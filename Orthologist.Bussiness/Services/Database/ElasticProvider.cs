using Nest;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Helper;
using System;

namespace Orthologist.Bussiness.Services.Database
{
    public class ElasticProvider : IDatabaseProvider
    {
        ElasticClient _elasticClient;

        public ElasticProvider()
        {
            _elasticClient = new ElasticClient(GetConnectionForIndex("orthoanalyze"));
        }

        public async Task<T> GetOrthoObjectAsync<T>(string id) where T : BaseDatabaseObject
        {
            var result = await _elasticClient.SearchAsync<T>(x => x.Index(typeof(T).Name.ToLower()).Query(y =>
                y.Match(m => m.Field(f=> f.Name.Suffix("keyword")).Query(id))));
            if (!result.IsValid) return null; 
            if (result.Documents.Count > 0) return result.Documents.First();
            return null;
        }

        public T GetOrthoObject<T>(string id) where T : BaseDatabaseObject
        {            var result = _elasticClient.Search<T>(x => x.Index(typeof(T).Name.ToLower()).Query(y =>
                y.Match(m => m.Field(f => f.Name.Suffix("keyword")).Query(id))));
            if (!result.IsValid) return null;
            if (result.Documents.Count > 0) return result.Documents.First();
            return null;
        }

        public async Task<List<T>> GetAllOrthoObjectsAsync<T>() where T : BaseDatabaseObject
        {
            List<T> results = new List<T>();
            var searchResponse = await _elasticClient.SearchAsync<T>(s => s
                .Index(typeof(T).Name.ToLower())
                .Size(5000)
                .Scroll("2m")
            );

            while (searchResponse.Documents.Count > 0)
            {
                foreach (var document in searchResponse.Documents)
                {
                    results.Add(document);
                }
                searchResponse = await _elasticClient.ScrollAsync<T>("2m", searchResponse.ScrollId);
            }
            return results;
        }

        public async Task<bool> CreateOrthoObjectAsync<T>(T obj) where T : BaseDatabaseObject
        {
            var result = await _elasticClient.IndexAsync(obj, i => i.Index(typeof(T).Name.ToLower()));
            return result.IsValid;
        }

        public async Task<bool> UpdateOrthoObjectAsync<T>(T obj) where T : BaseDatabaseObject
        {
            var result = await DeleteOrthoObjectAsync<T>(obj.Id.ToString());
            if (result)
            {
                var indexResult = await CreateOrthoObjectAsync(obj);
                return indexResult;
            }
            return false;
        }

        public async Task<bool> DeleteOrthoObjectAsync<T>(string id) where T : BaseDatabaseObject
        {
            var result = await _elasticClient.DeleteByQueryAsync<T>(x => x.Index(typeof(T).Name.ToLower()).Query(q => q.Match(m => m.Field(f=> f.Name.Suffix("keyword")).Query(id))));
            return result.IsValid;
        }

        private ConnectionSettings GetConnectionForIndex(string defaultIndex)
        {
            return new ConnectionSettings(new Uri("http://localhost:9200"))
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                .BasicAuthentication("elastic", "=wdzhtb6-sgmr792h-SA")
                .DefaultIndex(defaultIndex);
        }

        public async Task<(List<OrthoGroupForAnalyze>, long)> GetOrthogroupsByOrganisms(IList<string> organismNames, int page, int pageNum)
        {
            var countResponse = await _elasticClient.CountAsync<OrthoGroupForAnalyze>(s => s
    .Index(typeof(OrthoGroupForAnalyze).Name.ToLower())
    .Query(q => q.Bool(b => b
        .Filter(organismNames.Select(organism => (Func<QueryContainerDescriptor<OrthoGroupForAnalyze>, QueryContainer>)(f =>
            f.Term(t => t.Field("organisms.keyword").Value(organism))
        )).ToArray())
    )));
            var searchResponse = await _elasticClient.SearchAsync<OrthoGroupForAnalyze>(s => s
    .Index(typeof(OrthoGroupForAnalyze).Name.ToLower())
    .Query(q => q.Bool(b => b
        .Filter(organismNames.Select(organism => (Func<QueryContainerDescriptor<OrthoGroupForAnalyze>, QueryContainer>)(f =>
            f.Term(t => t.Field("organisms.keyword").Value(organism))
        )).ToArray())
    )).Size(page).Skip((page * pageNum) - pageNum)
);
            return (searchResponse.Documents.ToList(), countResponse.Count);
        }

        public async Task<(List<OrthoGroupForAnalyze>, long)> GetOrthogroupsByOrganisms(IList<string> organismNames, int page, int pageNum, IList<FilterItem> filters)
        {
            var organismFilters = organismNames
    .Select(organism => new QueryContainerDescriptor<OrthoGroupForAnalyze>()
        .Term(t => t.Field("organisms.keyword").Value(organism)))
    .ToList();

            //var orthogroupFilter = new QueryContainerDescriptor<OrthoGroupForAnalyze>()
            //    .Terms(t => t
            //        .Field("orthogroupsNames.keyword")
            //        .Terms(filters.First().Value.Replace("\t", "")));

            // Kombinace filtrů
            var combinedFilters = new List<QueryContainer>();
            combinedFilters.AddRange(organismFilters);
            //combinedFilters.Add(orthogroupFilter);
            

            var famFilter = filters.FirstOrDefault(x => x.PropertyName == "FamilyName");
            if (famFilter != null)
            {
                var familiyFilter = new QueryContainerDescriptor<OrthoGroupForAnalyze>()
                    .Term(t => t.Field("taxonName.keyword").Value(famFilter.PropertyValue));
                combinedFilters.Add(familiyFilter);
            }

            var countResponse = await _elasticClient.CountAsync<OrthoGroupForAnalyze>(s => s
                .Index(typeof(OrthoGroupForAnalyze).Name.ToLower())
                .Query(q => q.Bool(b => b.Filter(combinedFilters.ToArray())))
            );

            var searchResponse = await _elasticClient.SearchAsync<OrthoGroupForAnalyze>(s => s
                .Index(typeof(OrthoGroupForAnalyze).Name.ToLower())
                .Query(q => q.Bool(b => b.Filter(combinedFilters.ToArray())))
                .Size(page)
                .Skip(page * (pageNum - 1))
            );
            return (searchResponse.Documents.ToList(), countResponse.Count);
        }

        public async Task<List<OrthoGroupForAnalyze>> GetOrthoGroupForFastaUpdate(int count)
        {
            var searchResponse = await _elasticClient.SearchAsync<OrthoGroupForAnalyze>(s => s
    .Index(typeof(OrthoGroupForAnalyze).Name.ToLower())
    .Query(q => q
        .Bool(b => b
            .MustNot(mn => mn
                .Exists(e => e
                    .Field(f => f.GeneRecords)
                )
            )
        )
    )
    .Size(count)
);
            if (searchResponse != null && searchResponse.IsValid)
            {
                return searchResponse.Documents.ToList();
            }
            return null;
        }

        public async Task<TaxTree> LoadTree()
        {
            var result = await _elasticClient.SearchAsync<TaxTree>(x => x.Index("taxtree").Query(y => y.MatchAll()));
            if (result.Documents.Count > 0) return result.Documents.First();
            return null;
        }

        public async Task<bool> SaveTree(TaxTree tree)
        {
            var result = await _elasticClient.IndexAsync(tree, i => i.Index("taxtree"));
            return result.IsValid;
        }
    }
}
