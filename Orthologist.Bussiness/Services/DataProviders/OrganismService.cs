using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Helper;
using Orthologist.Bussiness.Services.Database;
using Orthologist.Bussiness.Services.Ebi;
using Orthologist.Bussiness.Services.OrthoDb;
using System.Xml.Linq;
using static Nest.JoinField;

namespace Orthologist.Bussiness.Services.DataProviders
{
    public class OrganismService : IOrganismService
    {
        private IDatabaseProvider _provider;
        private IOrthoTreeService _orthoTreeService;
        private ILineageService _lineageService;
        private ProfileConfig _profileConfig;

        public OrganismService(IDatabaseProvider provider, IOrthoTreeService orthoTreeService, ILineageService lineageService, IOptions<ProfileConfig> profileConfig)
        {
            _provider = provider;
            _profileConfig = profileConfig.Value;
            _orthoTreeService = orthoTreeService;
            _lineageService = lineageService;
        }

        public async Task<string> GetTreeForOrganismsAsync(List<string> organismList)
        {
            List<Organism> organisms = new List<Organism>();
            foreach (var org in organismList)
            {
                var organism = await GetOrganism(org);
                organism.IsMain = true;
                organisms.Add(organism);
            }
            return JsonConvert.SerializeObject(SetTreeForOrganisms(organisms));
        }

        public async Task<List<Organism>> GetTreeForOrganismsAsync(Dictionary<string, string> organismList)
        {
            List<Organism> organisms = new List<Organism>();
            foreach (var org in organismList)
            {
                var organism = await GetOrganism(org.Key);
                if (organism != null)
                {
                    organism.IsMain = true;
                    organisms.Add(organism);
                }
            }
            return organisms;
        }

        public async Task<string> GetTreeForOrganismsWithSimilarAsync(List<string> organismList)
        {
            List<Organism> organisms = new List<Organism>();
            foreach (var org in organismList)
            {
                var organism = await GetOrganism(org);
                organism.IsMain = true;
                var orthoOrganisms = await GetSimilarOrganisms(organism, organismList);
                if (orthoOrganisms != null)
                {
                    foreach (var oo in orthoOrganisms)
                    {
                        if (!organisms.Select(x => x.Name).Contains(oo.Name))
                        {
                            oo.IsMain = false;
                            organisms.Add(oo);
                        }
                    }
                }
                organisms.Add(organism);
            }
            var taxItem = SetTreeForOrganisms(organisms);
            return JsonConvert.SerializeObject(taxItem);
        }

        public async Task<(List<OrthoGroupForAnalyze>, long)> GetOrthoGroupsForAnalyze(List<string> organismList, int page, int pageNum, IEnumerable<FilterItem> filters = null)
        {
            var filterlist = filters.ToList();
            var orthoGroups = !filterlist.Any() ? await _provider.GetOrthogroupsByOrganisms(organismList, page, pageNum) : await _provider.GetOrthogroupsByOrganisms(organismList, page, pageNum, filterlist);
            return orthoGroups;
        }

        private TaxItem SetRelativeTreeLeaf(Organism organism)
        {
            TaxItem outsiderNode = new TaxItem() { key = organism.Lineage[organism.Lineage.Count - 2], name = organism.Lineage[organism.Lineage.Count - 2], children = new List<TaxItem>(), Main = organism.IsMain };
            outsiderNode.children.Add(new TaxItem() { key = organism.TaxId, name = organism.Name, children = new List<TaxItem>(), Main = organism.IsMain });
            return outsiderNode;
        }

        private TaxItem GetOutsiders(IList<string> maxLineage, ref List<Organism> organisms)
        {
            TaxItem outsiderNode = new TaxItem() { children = new List<TaxItem>() };
            for (int i = 1; i < maxLineage.Count; i++)
            {
                int count = organisms.Count(x => !x.Lineage.Contains(maxLineage[i]));
                if (count > 0)
                {
                    outsiderNode.name = maxLineage[i - 1];
                    outsiderNode.key = maxLineage[i - 1];
                    if (count == 1)
                    {
                        var node = SetRelativeTreeLeaf(organisms.FirstOrDefault(x => !x.Lineage.Contains(maxLineage[i])));
                        if (node.name == outsiderNode.name)
                        {
                            outsiderNode.children.Add(node.children[0]);
                        }
                        else { outsiderNode.children.Add(node); }
                        organisms = organisms.Where(x => x.Lineage.Contains(maxLineage[i])).ToList();
                        return outsiderNode;
                    }
                    else
                    {
                        //Musí se přehodit kolekce, aby byl outsider jen jeden
                        var similarityNode = CheckGenusSimilarity(organisms.Where(x => !x.Lineage.Contains(maxLineage[i])).ToList());
                        if (similarityNode != null)
                        {
                            organisms = organisms.Where(x => x.Lineage.Contains(maxLineage[i])).ToList();
                            return similarityNode;
                        }
                        var org = organisms.FirstOrDefault(x => !x.Lineage.Contains(maxLineage[i]));
                        organisms.Remove(org);
                        var node = SetRelativeTreeLeaf(org);
                        if (node.name == outsiderNode.name)
                        {
                            outsiderNode.children.Add(node.children[0]);
                        }
                        else { outsiderNode.children.Add(node); }
                        //organisms = organisms.Where(x => !x.Lineage.Contains(maxLineage[i])).ToList();
                        return outsiderNode;
                    }
                }
                else
                {
                    organisms.ForEach(x => x.Lineage = x.Lineage.Skip(1).ToList());
                }
            }
            return outsiderNode;
        }

        private TaxItem CheckGenusSimilarity(List<Organism> organisms)
        {
            TaxItem outsiderNode = new TaxItem() { children = new List<TaxItem>() };
            if (organisms.Count(x => x.Lineage.Last() == organisms[0].Lineage.Last()) == organisms.Count)
            {
                var child = new TaxItem() { key = organisms[0].Lineage.Last(), name = organisms[0].Lineage.Last(), children = new List<TaxItem>() };
                foreach (var org in organisms)
                {
                    child.children.Add(new TaxItem() { key = org.TaxId, name = org.Name, children = new List<TaxItem>() });
                }
                outsiderNode.children.Add(child);
                organisms = new List<Organism>();
                return outsiderNode;
            }
            return null;
        }

        private TaxItem SetTreeForOrganisms(List<Organism> organisms)
        {
            TaxItem rootNode = new TaxItem() { key = _profileConfig.ProfileId, name = _profileConfig.ProfileTaxon, children = new List<TaxItem>() };
            foreach (var org in organisms)
            {
                SetOrganismToTree(rootNode, org);
            }
            return rootNode;
        }

        private void SetOrganismToTree(TaxItem rootNode, Organism org)
        {
            TaxItem lastNode = rootNode;
            int index = org.Lineage.IndexOf(rootNode.name);
            foreach (var lin in org.Lineage.Skip(index + 1).ToList())
            {
                TaxItem nextNode = SearchItem(lin, lastNode);
                if (nextNode != null)
                {
                    lastNode = nextNode;
                }
                else
                {
                    index = org.Lineage.IndexOf(lin);
                    foreach (var linins in org.Lineage.Skip(index).ToList())
                    {
                        lastNode.children.Add(new TaxItem() { name = linins, children = new List<TaxItem>() });
                        lastNode = lastNode.children.FirstOrDefault(x => x.name == linins);
                    }
                    lastNode.children.Add(new TaxItem() { name = org.Name, key = org.TaxId });
                    return;

                }
            }
            lastNode.children.Add(new TaxItem() { name = org.Name, key = org.TaxId });
        }

        private TaxItem SearchItem(string nodeId, TaxItem node)
        {
            if (node == null)
            {
                return null;
            }

            if (node.children != null && node.children.Any())
            {
                var matchingChild = node.children.FirstOrDefault(x => x.name == nodeId);
                if (matchingChild != null)
                {
                    return new TaxItem(matchingChild, true)
                    {
                        children = matchingChild.children
                    };
                }

                foreach (var child in node.children)
                {
                    var result = SearchItem(nodeId, child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        private TaxItem SetRelativeTree(List<Organism> organisms)
        {
            var lineage = GetMaxLineage(organisms);
            List<Organism> outsiders = new List<Organism>();
            var root = GetOutsiders(lineage, ref organisms);
            SetRelativeTreeNodes(organisms, root);
            return root;
        }

        private void SetRelativeTreeNodes(List<Organism> organisms, TaxItem root)
        {
            while (organisms.Count > 1)
            {
                var lineage = GetMaxLineage(organisms);
                if (lineage.Count > 0)
                {
                    var baseNode = FindNodeByName(root, lineage[0]);
                    var node = GetOutsiders(lineage, ref organisms);
                    if (node.name == baseNode.name)
                    {
                        if (baseNode.children.Any())
                        {
                            var hlpNode = baseNode.children.FirstOrDefault(x => x.name == node.children[0].name);
                            if (hlpNode != null && node.children[0].children.Any())
                            {
                                hlpNode.children.Add(node.children[0].children[0]);
                            }
                            else
                            {
                                baseNode.children.Add(node.children[0]);
                            }
                        }
                        else
                        {
                            baseNode.children.Add(node);
                        }
                    }
                    else
                    {
                        baseNode.children.Add(node);
                    }
                }
                else
                {
                    root.children.Add(new TaxItem() { key = organisms[0].TaxId, name = organisms[0].Name, Main = organisms[0].IsMain });
                    organisms.Remove(organisms[0]);
                }
            }
            var lastNode = FindNodeInLineage(root, organisms[0].Lineage);
            if (lastNode != null)
            {
                if (lastNode.name == organisms[0].Lineage[organisms[0].Lineage.Count - 2])
                {
                    lastNode.children.Add(new TaxItem() { key = organisms[0].TaxId, name = organisms[0].Name, Main = organisms[0].IsMain });
                }
                else
                {
                    var child = new TaxItem() { key = organisms[0].Lineage[organisms[0].Lineage.Count - 2], name = organisms[0].Lineage[organisms[0].Lineage.Count - 2], Main = organisms[0].IsMain, children = new List<TaxItem>() };
                    child.children.Add(new TaxItem() { key = organisms[0].TaxId, name = organisms[0].Name, children = new List<TaxItem>(), Main = organisms[0].IsMain });
                    lastNode.children.Add(child);
                }
            }
            else
            {
                var child = new TaxItem() { key = organisms[0].Lineage[organisms[0].Lineage.Count - 2], name = organisms[0].Lineage[organisms[0].Lineage.Count - 2], children = new List<TaxItem>(), Main = organisms[0].IsMain };
                child.children.Add(new TaxItem() { key = organisms[0].TaxId, name = organisms[0].Name, children = new List<TaxItem>(), Main = organisms[0].IsMain });
                root.children.Add(child);
            }
        }


        private TaxItem FindNodeInLineage(TaxItem root, IList<string> lineage)
        {
            var baseNode = FindNodeByName(root, lineage[0]);
            for (int i = 1; i < lineage.Count; i++)
            {
                var newNode = FindNodeByName(root, lineage[i]);
                if (newNode != null) baseNode = newNode;
            }
            return baseNode;
        }

        private TaxItem FindNodeByName(TaxItem root, string hledanyString)
        {
            if (root.name == hledanyString)
            {
                return root;
            }

            if (root.children != null)
            {
                foreach (var child in root.children)
                {
                    var result = FindNodeByName(child, hledanyString);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private IList<string> GetMaxLineage(IList<Organism> organisms)
        {
            for (int i = 0; i < organisms.Min(x => x.Lineage.Count); i++)
            {
                var outsiders = organisms.Where(x => x.Lineage[i] != organisms[0].Lineage[i]);
                var nonoutsiders = organisms.Where(x => x.Lineage[i] == organisms[0].Lineage[i]);
                if (outsiders.Any())
                {
                    return outsiders.Count() > nonoutsiders.Count() ? outsiders.First().Lineage : nonoutsiders.First().Lineage;
                }
            }
            return new List<string>();
        }

        private async Task<Organism> GetOrganism(string name)
        {
            var organism = await _provider.GetOrthoObjectAsync<Organism>(name);
            if (organism == null)
            {
                organism = await _lineageService.GetInfosForOrganism(name);
                if (organism != null)
                {
                    organism.InOrthoDb = _orthoTreeService.LoadSpecies().Any(x => x == name);
                    _provider.CreateOrthoObjectAsync(organism);
                }
            }
            return organism;
        }

        private async Task<List<Organism>> GetSimilarOrganisms(Organism organism, IList<string> selectedOrganisms)
        {
            string lastfamiliy = null;
            List<Organism> organisms = new List<Organism>();
            if (organism != null && organism.Lineage != null)
            {
                lastfamiliy = organism.Lineage.FirstOrDefault(x => x.EndsWith("idae"));
                if (lastfamiliy != null)
                {
                    var result = await _orthoTreeService.LoadMembersForFamily(lastfamiliy);
                    if (result != null)
                    {
                        foreach (var member in result)
                        {
                            if (!selectedOrganisms.Contains(member))
                            {
                                string updatedName = $"{member.Split(' ')[0]} {member.Split(' ')[1]}".TrimEnd(',');
                                var orthoOrganism = await _provider.GetOrthoObjectAsync<Organism>(updatedName);
                                if (orthoOrganism != null)
                                {
                                    if (!organisms.Select(x => x.Name).Contains(updatedName))
                                    {
                                        organisms.Add(orthoOrganism);
                                    }
                                }
                            }
                        }
                        return organisms;
                    }
                }
            }
            return null;
        }
    }
}
