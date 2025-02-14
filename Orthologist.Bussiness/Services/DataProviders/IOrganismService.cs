using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Helper;

namespace Orthologist.Bussiness.Services.DataProviders
{
    public interface IOrganismService
    {
        Task<string> GetTreeForOrganismsAsync(List<string> organismList);
        Task<List<Organism>> GetTreeForOrganismsAsync(Dictionary<string, string> organismList);
        Task<string> GetTreeForOrganismsWithSimilarAsync(List<string> organismList);
        Task<(List<OrthoGroupForAnalyze>, long)> GetOrthoGroupsForAnalyze(List<string> organismList, int page, int pageNum, IEnumerable<FilterItem> filters = null);
    }
}