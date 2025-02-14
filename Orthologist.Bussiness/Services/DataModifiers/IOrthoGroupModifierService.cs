using Orthologist.Bussiness.Classes;

namespace Orthologist.Bussiness.Services.DataModifiers
{
    public interface IOrthoGroupModifierService
    {
        Task<bool> CreateMantelTresholdForOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, bool withZscore = false);
        Task<bool> CreatePermanovaTresholdForOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze);
        Task<OrthoGroupAnalyze> WriteTreeToOrthoGroup(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze = null);
        Task<bool> CreateMatrixesForCanidateGeneMantel(OrthoGroupForAnalyze groupForAnalyze, GeneRecord candidate, string orgToLeft);
        Task<bool> CreateZscoreMatrixesForCanidateGeneMantel(OrthoGroupForAnalyze groupForAnalyze, GeneRecord candidate, string orgToLeft);
        Task WriteTreeToOrthoGroups();
    }
}