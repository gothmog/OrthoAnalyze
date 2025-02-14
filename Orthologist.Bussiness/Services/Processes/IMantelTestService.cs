using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;

namespace Orthologist.Bussiness.Services.Processes
{
    public interface IMantelTestService
    {
        Task<MantelResponse> ProcessTestWithCandidate(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, GeneRecord candidateGene, GeneRecord closestGene);
    }
}