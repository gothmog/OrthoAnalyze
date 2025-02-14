using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;

namespace Orthologist.Bussiness.Services.Processes
{
    public interface IPermanovaTestService
    {
        Task<PermanovaResponse> ProcessTestWithCandidate(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, GeneRecord candidateGene);
    }
}