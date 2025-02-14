using Orthologist.Bussiness.Classes;

namespace Orthologist.Bussiness.Services.DataProviders
{
    public interface IOrthoGroupService
    {
        Task<OrthoGroupAnalyze> GetOrthoGeneGroupAnalyzeAsync(string groupId);
        Task<OrthoGroupForAnalyze> GetOrthoGroupMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate);
        Task<OrthoGroupForAnalyze> GetOrthoGroupZscoreMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate);
        Task<OrthoGroupForAnalyze> GetOrthoGeneGroupAsync(string groupId);
        Task SetTrasholdGroup(string groupId);
        Task SetTestGroup(string groupId, string orgToLeft);
    }
}