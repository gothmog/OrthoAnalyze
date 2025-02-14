using Orthologist.Bussiness.Classes;
using Orthologist.Web.Models;

namespace Orthologist.Web.Services.Bussiness
{
    public interface IOrthoFamilyService
    {
        Task<OrthoGroupAnalyzeDto> GetOrthoGroupAnalyzeDtoAsync(string groupId);
        Task GetOrthoGroupMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate);
        Task GetOrthoGroupZscoreMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate);
        OrthoGroupDto GetOrthoGroupDto(string groupId);
        void SetTestGroup(string groupId, string orgToLeft);
        void SetTrasholdGroup(string groupId);
        Task<OrthoGroupDto> GetOrthoGroupDtoAsync(string groupId);
    }
}