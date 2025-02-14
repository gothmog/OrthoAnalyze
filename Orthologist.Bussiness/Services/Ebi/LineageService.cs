using Newtonsoft.Json;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Helper;

namespace Orthologist.Bussiness.Services.Ebi
{
    public class LineageService : ILineageService
    {
        private string url = "https://www.ebi.ac.uk/ena/taxonomy/rest/scientific-name/";
        private HttpClient client;

        public LineageService()
        {
            client = new HttpClient() { BaseAddress = new Uri(url) };
        }

        public async Task<Organism> GetInfosForOrganism(string organismName)
        {
            var json = await client.GetStringAsync(organismName);
            if (json != null)
            {
                var taxonomyInfos = JsonConvert.DeserializeObject<List<EbiTaxonomyInfo>>(json);
                if (taxonomyInfos != null && taxonomyInfos.Any())
                {
                    var taxonomyInfo = taxonomyInfos[0];
                    taxonomyInfo.LineageList = taxonomyInfo.Lineage.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(y => y.TrimStart(' ')).Where(x => !String.IsNullOrEmpty(x)).ToList();
                    return new Organism() 
                    {  
                        CommonName = taxonomyInfo.CommonName, 
                        Created = DateTime.Now, 
                        FormalName = taxonomyInfo.FormalName, 
                        Lineage = taxonomyInfo.LineageList,
                        Rank = taxonomyInfo.Rank,
                        Name = taxonomyInfo.ScientificName,
                        TaxId = taxonomyInfo.TaxId
                    };
                }
            }
            return null;
        }
    }
}
