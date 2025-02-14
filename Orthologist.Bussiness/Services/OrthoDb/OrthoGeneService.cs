using Nest;
using Newtonsoft.Json;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;
using Orthologist.Bussiness.Services.Database;
using Orthologist.Bussiness.Services.DataModifiers;
using Orthologist.Bussiness.Services.DataProviders;
using Orthologist.Bussiness.Services.Helpers;
using Orthologist.Bussiness.Services.Statistics;
using RDotNet;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Orthologist.Bussiness.Services.OrthoDb
{
    /// <summary>
    /// Služba, která má za úkol ukládat a pracovat s uloženými OrthoGroupami a jejich různé modifikace
    /// </summary>
    public class OrthoGeneService : IOrthoGeneService
    {
        private IList<OrthoGroupForAnalyze> _groupsForAnalyze;
        private string _url = "https://data.orthodb.org/v12/";
        IOrganismService _organismService;
        IDatabaseProvider _databaseProvider;
        IOrthoTreeService _orthoTreeService;
        HttpClient _httpClient;
        long count = 0;

        public OrthoGeneService(IOrganismService organismService, IDatabaseProvider databaseProvider, IOrthoTreeService orthoTreeService)
        {
            _organismService = organismService;
            _databaseProvider = databaseProvider;
            _orthoTreeService = orthoTreeService;
            _httpClient = new HttpClient() { BaseAddress = new Uri(_url) };
        }

        /// <summary>
        /// Metoda prochází uložené Orthogroupy, které nemají vyplněné GeneRecords a postupně je doplní
        /// </summary>
        /// <returns></returns>
        public async Task WriteFastaToOrthoGroups()
        {
            _groupsForAnalyze = await _databaseProvider.GetOrthoGroupForFastaUpdate(20);
            while (_groupsForAnalyze != null && _groupsForAnalyze.Any())
            {
                count = count + _groupsForAnalyze.Count;
                if (_groupsForAnalyze != null && _groupsForAnalyze.Any())
                {
                    Parallel.ForEach(_groupsForAnalyze, async (group) =>
                    {
                        await WriteFastaToOrthoGroup(group);
                    });
                }
                _groupsForAnalyze = await _databaseProvider.GetOrthoGroupForFastaUpdate(20);
            }
        }

        private async Task WriteFastaToOrthoGroup(OrthoGroupForAnalyze group)
        {
            string fastaText = await _httpClient.GetStringAsync($"fasta?id={group.Name}");
            var geneRecords = OrthoGeneParser.GetFastaRecords(fastaText);
            if (geneRecords != null)
            {
                group.GeneRecords = geneRecords;
                group.ModificationDate = DateTime.Now;
                await _databaseProvider.UpdateOrthoObjectAsync(group);
            }
        }

        public async Task LoadAllOrganismsOrthoGroupsWithFasta(string taxonName)
        {
            Dictionary<string, string> orthoOrganisms = await _orthoTreeService.GetAllOrganismForTaxon(taxonName);
            IList<Organism> organisms = await _organismService.GetTreeForOrganismsAsync(orthoOrganisms);
            await GetOrthoGroupsForTaxonUnit(taxonName, organisms);
        }

        private async Task GetOrthoGroupsForTaxonUnit(string taxonName, IList<Organism> organisms)
        {
            var taxons = DataTablesProvider.GetTaxons();
            foreach (var org in organisms) 
            {
                var dbOrg = await _databaseProvider.GetOrthoObjectAsync<OrganismTaxUnitModel>(org.Name);
                if (dbOrg == null)
                {
                    bool result = true;
                    var hlpLineage = org.Lineage.Skip(org.Lineage.IndexOf(taxonName)).ToList();
                    foreach (var taxon in hlpLineage)
                    {
                        var unit = taxons.FirstOrDefault(x => x.Name == taxon);
                        if (unit != null)
                        {
                            result = await GetOrthoGroupForOrganismAndUnit(org.TaxId, unit.Id);
                        }
                    }
                    if (result)
                    {
                        await _databaseProvider.CreateOrthoObjectAsync(new OrganismTaxUnitModel() { Created = DateTime.Now, Name = org.Name, TaxUnitName = taxonName });
                    }
                }
            }
        }

        private async Task<bool> GetOrthoGroupForOrganismAndUnit(string taxId, string unitId)
        {
            string _jsonCount = await _httpClient.GetStringAsync($"search?query={taxId}&level={unitId}&counts_only=1");
            var unitCount = JsonConvert.DeserializeObject<OrthoGroupCount>(_jsonCount);
            if (unitCount != null)
            {
                for (int i = 0; i  < unitCount.Count; i = i + 100)
                {
                    try
                    {
                        string _jsonData = await _httpClient.GetStringAsync($"search?query={taxId}&level={unitId}&skip={i}&take=100");
                        var unitData = JsonConvert.DeserializeObject<OrthoGroupData>(_jsonData);
                        if (unitData != null)
                        {
                            foreach (var item in unitData.Data)
                            {
                                var oldGroup = _groupsForAnalyze.FirstOrDefault(x=> x.Name == item);
                                if (oldGroup == null)
                                {
                                    var group = new OrthoGroupForAnalyze() { Created = DateTime.Now, Name = item };
                                    await _databaseProvider.CreateOrthoObjectAsync(group);
                                    _groupsForAnalyze.Add(group);
                                }
                            }
                        }
                        Task.Delay(100).Wait();
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }

    public class OrthoGroupCount
    {
        public long Count { get; set; }
    }

    public class OrthoGroupData
    {
        public List<string> Data { get; set; }
    }
}
