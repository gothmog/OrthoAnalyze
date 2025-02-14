using BlazorBootstrap;
using Orthologist.Bussiness.Services.DataProviders;
using Orthologist.Web.Models;

namespace Orthologist.Web.Services.Bussiness
{
    public class OrganismSelectionService : IOrganismSelectionService
    {
        private IOrganismService _organismService;

        public OrganismSelectionService(IOrganismService organismService)
        {
            _organismService = organismService;
        }

        public async Task<string> GetTreeForOrganismsAsync(List<string> organismList)
        {
            return await _organismService.GetTreeForOrganismsAsync(organismList);
        }

        public async Task<string> GetTreeForOrganismsWithSimilarAsync(List<string> organismList)
        {
            return await _organismService.GetTreeForOrganismsWithSimilarAsync(organismList);
        }

        public async Task<OrthoGroupResult> GetOrthoGroupsDtos(List<string> organismList, int page, int pageNum, IEnumerable<FilterItem> filters = null)
        {
            if (organismList == null || !organismList.Any()) return new OrthoGroupResult() { TotalCount = 0, Groups = new List<OrthoGroupDto>() };
            var filterlist = filters.ToList();
            var orthoGroups = await _organismService.GetOrthoGroupsForAnalyze(organismList, page, pageNum, filterlist.Select(x => new Orthologist.Bussiness.Classes.Helper.FilterItem() { PropertyValue = x.Value, PropertyName = x.PropertyName }));
            if (orthoGroups.Item1.Any())
            {
                return new OrthoGroupResult()
                {
                    Groups = orthoGroups.Item1.Select(x => new OrthoGroupDto(x)).ToList(),
                    TotalCount = orthoGroups.Item2,
                };
            }
            return new OrthoGroupResult() { TotalCount = 0, Groups = new List<OrthoGroupDto>() };
        }
    }
}
