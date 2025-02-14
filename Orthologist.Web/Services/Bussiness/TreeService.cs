using Orthologist.Bussiness.Services.OrthoDb;

namespace Orthologist.Web.Services.Bussiness
{
    public class TreeService : ITreeService
    {
        IOrthoTreeService _orthoTreeService;

        public TreeService(IOrthoTreeService orthoTreeService)
        {
            _orthoTreeService = orthoTreeService;
        }

        public Task EnsureInitializedAsync()
        {
            return _orthoTreeService.EnsureInitializedAsync();
        }

        public IList<string> LoadSpecies()
        {
            return _orthoTreeService.LoadSpecies();
        }

        public async Task<string> LoadSubTree(string nodeId)
        {
            return await _orthoTreeService.LoadSubTree(nodeId);
        }

        public Task<string> GetJsonForGraph()
        {
            return _orthoTreeService.GetJsonForGraph();
        }

        public IList<string> LoadTaxons()
        {
            return _orthoTreeService.LoadTaxons();
        }
    }
}
