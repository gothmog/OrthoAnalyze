using BlazorBootstrap;
using Orthologist.Web.Models;

namespace Orthologist.Web.Services.Bussiness
{
    public interface IOrganismSelectionService
    {
        Task<OrthoGroupResult> GetOrthoGroupsDtos(List<string> organismList, int page, int pageNum, IEnumerable<FilterItem> filters = null);
        Task<string> GetTreeForOrganismsAsync(List<string> organismList);
        Task<string> GetTreeForOrganismsWithSimilarAsync(List<string> organismList);
    }
}