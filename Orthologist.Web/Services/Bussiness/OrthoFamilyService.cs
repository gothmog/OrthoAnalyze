using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Services.DataProviders;
using Orthologist.Web.Models;

namespace Orthologist.Web.Services.Bussiness
{
    public class OrthoFamilyService : IOrthoFamilyService
    {
        IOrthoGroupService _orthoGroupService;
        public OrthoFamilyService(IOrthoGroupService orthoGroupService)
        {
            _orthoGroupService = orthoGroupService;
        }

        public OrthoGroupDto GetOrthoGroupDto(string groupId)
        {
            return GetOrthoGroupDtoAsync(groupId).Result;
        }

        public async Task<OrthoGroupDto> GetOrthoGroupDtoAsync(string groupId)
        {
            return new OrthoGroupDto(await _orthoGroupService.GetOrthoGeneGroupAsync(groupId));
        }

        public async Task GetOrthoGroupMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate)
        {
            await _orthoGroupService.GetOrthoGroupMatrixComparationAsync(groupId, orgToLeft, candidate);
        }

        public async Task GetOrthoGroupZscoreMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate)
        {
            await _orthoGroupService.GetOrthoGroupZscoreMatrixComparationAsync(groupId, orgToLeft, candidate);
        }

        public async Task<OrthoGroupAnalyzeDto> GetOrthoGroupAnalyzeDtoAsync(string groupId)
        {
            var orthoGroup = await _orthoGroupService.GetOrthoGeneGroupAnalyzeAsync(groupId);
            return new OrthoGroupAnalyzeDto(orthoGroup);
        }

        public async void SetTestGroup(string groupId, string orgToLeft)
        {
            await _orthoGroupService.SetTestGroup(groupId, orgToLeft);
        }

        public async void SetTrasholdGroup(string groupId)
        {
            await _orthoGroupService.SetTrasholdGroup(groupId);
        }
    }
}
