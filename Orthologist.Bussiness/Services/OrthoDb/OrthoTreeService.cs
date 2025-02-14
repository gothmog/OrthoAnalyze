using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Services.Database;
using System.Text.Json;

namespace Orthologist.Bussiness.Services.OrthoDb
{
    public class OrthoTreeService : IOrthoTreeService
    {
        private string _url = "https://data.orthodb.org/v12/";
        private string _json;
        private TaxTree taxTree;
        private IList<string> _taxons = new List<string>();
        private IList<string> _species = new List<string>();
        IDatabaseProvider _provider;
        ProfileConfig _profileConfig;

        private readonly Lazy<Task> _initializationTask;

        public OrthoTreeService(IDatabaseProvider provider, IOptions<ProfileConfig> profileConfig)
        {
            _provider = provider;
            _profileConfig = profileConfig.Value;
            _initializationTask = new Lazy<Task>(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            taxTree = await _provider.LoadTree();
            if (taxTree == null)
            {
                var client = new HttpClient() { BaseAddress = new Uri(_url) };
                _json = await client.GetStringAsync("tree");
                taxTree = JsonConvert.DeserializeObject<TaxTree>(_json);
                var profileTree = await LoadProfileTree(taxTree);
                await _provider.SaveTree(profileTree!);
            }
            GetAllTaxonsFromList();
            GetAllOrganismsFromList();
        }

        public Task EnsureInitializedAsync()
        {
            return _initializationTask.Value;
        }

        public async Task<TaxTree> LoadProfileTree(TaxTree taxTree)
        {
            var node = SearchItem(_profileConfig.ProfileTaxon, new TaxItem() { key = "1", name = "Base", children = taxTree.Data });
            if (node != null) 
            {
                return new TaxTree() { Data = new List<TaxItem> { node } };
            }
            return null;
        }

        public async Task<string> LoadSubTree(string nodeId)
        {
            var node = SearchItem(nodeId, new TaxItem() { key = _profileConfig.ProfileId, name = _profileConfig.ProfileTaxon, children = taxTree.Data });
            node = GetThreeOfTree(node, true);
            string text = JsonConvert.SerializeObject(node);
            return text;
        }

        public async Task<IList<string>> LoadMembersForFamily(string familyName)
        {
            taxTree = await _provider.LoadTree();
            var node = SearchItemExact(familyName, new TaxItem() { key = _profileConfig.ProfileId, name = _profileConfig.ProfileTaxon, children = taxTree.Data });
            if(node != null)
            {
                List<string> taxons = new List<string>();
                GetAllTaxonsForNode(node, ref taxons);
                return taxons;
            }
            return null;
        }

        public async Task<Dictionary<string, string>> GetAllOrganismForTaxon(string taxonName)
        {
            if(taxTree == null)
            {
                await InitializeAsync();
            }
            var taxonNode = SearchItemExact(taxonName, new TaxItem() { key = _profileConfig.ProfileId, name = _profileConfig.ProfileTaxon, children = taxTree.Data });
            Dictionary<string, string> organisms = new Dictionary<string, string>();
            GetAllOrganismForNode(taxonNode, ref organisms);
            return CleanDictionary(organisms);
        }

        public Dictionary<string, string> CleanDictionary(Dictionary<string, string> allorganisms)
        {
            Dictionary<string, string> organisms = new Dictionary<string, string>();
            if (allorganisms != null)
            {
                foreach(var item in allorganisms)
                {
                    string name = $"{item.Key.Split(' ')[0]} {item.Key.Split(' ')[1]}".TrimEnd(',');
                    if (!organisms.ContainsKey(name))
                    {
                        organisms.Add(name, item.Value);
                    }
                }
            }
            return organisms;
        }

        private void GetAllOrganismForNode(TaxItem node, ref Dictionary<string, string> organisms)
        {
            if (node != null) 
            {
                if(node.children != null && node.children.Any())
                {
                    foreach(TaxItem child in node.children)
                    {
                        if (child.children == null)
                        {
                            if (!organisms.ContainsKey(child.name))
                            {
                                organisms.Add(child.name, child.key);
                            }
                        }
                        else GetAllOrganismForNode(child, ref organisms);
                    }
                }
            }
        }

        private void GetAllTaxonsForNode(TaxItem node, ref List<string> taxons)
        {
            if (node != null)
            {
                if (node.children != null && node.children.Any())
                {
                    foreach (TaxItem child in node.children)
                    {
                        if (child.children != null)
                        {
                            GetAllTaxonsForNode(child, ref taxons);
                        }
                        else 
                        { 
                            taxons.Add(child.name);
                        }
                    }
                }
            }
        }

        private TaxItem GetThreeOfTree(TaxItem newItem, bool withTaxons = true)
        {
            TaxItem taxItem = new TaxItem(newItem) { children = new List<TaxItem>() };
            foreach (var item in newItem.children != null && newItem.children.Any() ? withTaxons ? newItem.children : newItem.children.Where(x => x.children != null) : taxTree.Data)
            {
                TaxItem taxChildItem = new TaxItem(item) { children = new List<TaxItem>() };
                if (item.children != null)
                {
                    foreach (var item2 in withTaxons ? item.children : item.children.Where(x => x.children != null))
                    {
                        taxChildItem.children.Add(new TaxItem(item2));
                    }
                }
                taxChildItem.children = taxChildItem.children.Take(12).ToList();
                taxItem.children.Add(taxChildItem);
            }
            return taxItem;
        }

        private string GetBaseTree()
        {
            var treeItem = GetThreeOfTree(new TaxItem() { key = _profileConfig.ProfileId, name = _profileConfig.ProfileTaxon, children = new List<TaxItem>() }).children.First();
            return JsonConvert.SerializeObject(treeItem);
        }

        private void GetAllTaxonsFromList()
        {
            foreach(var item in taxTree.Data)
            {
                GetNestedTaxons(item);
            }
            _taxons = _taxons.Order().ToList();
        }

        private void GetNestedTaxons(TaxItem node)
        {
            if(node.children != null)
            {
                _taxons.Add(node.name);
                foreach(var item in node.children)
                {
                    GetNestedTaxons(item);
                }
            }
        }

        private void GetAllOrganismsFromList()
        {
            foreach (var item in taxTree.Data)
            {
                GetNestedOrganisms(item);
            }
            _species = _species.Order().ToList();
        }

        private void GetNestedOrganisms(TaxItem node)
        {
            if (node.children != null)
            {
                foreach (var item in node.children)
                {
                    GetNestedOrganisms(item);
                }
            }
            else
            {
                string name = node.name.Split(',')[0];
                if (!_species.Contains(name)) _species.Add(name);
            }
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

        private TaxItem SearchItemExact(string nodeId, TaxItem node)
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
                    return matchingChild;
                }

                foreach (var child in node.children)
                {
                    var result = SearchItemExact(nodeId, child);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }


        public async Task<string> GetJsonForGraph()
        {
            await EnsureInitializedAsync();
            return GetBaseTree();
        }

        public IList<string> LoadTaxons()
        {
            return _taxons;
        }

        public IList<string> LoadSpecies()
        {
            return _species;
        }
    }
}
