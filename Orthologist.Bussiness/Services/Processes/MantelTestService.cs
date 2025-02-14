using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;
using Orthologist.Bussiness.Services.DataModifiers;
using Orthologist.Bussiness.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Services.Processes
{
    public class MantelTestService : BaseTestService, IMantelTestService
    {
        IOrthoGroupModifierService _orthoGroupModifierService;

        public MantelTestService(IDataMatrixService dataMatrixService, IRStatisticService statisticService, IOrthoGroupModifierService orthoGroupModifierService) : base(dataMatrixService, statisticService)
        {
            _orthoGroupModifierService = orthoGroupModifierService;
        }

        public async Task<MantelResponse> ProcessTestWithCandidate(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, GeneRecord candidateGene, GeneRecord closestGene)
        {
            if(groupAnalyze.MantelOrthoGroupResult == null)
            {
                await _orthoGroupModifierService.CreateMantelTresholdForOrthoGroup(groupForAnalyze, groupAnalyze);
            }
            SwapGeneRecord(groupForAnalyze, closestGene, candidateGene);
            var derivedMatrix = _dataMatrixService.GenerateDistanceMatrix(groupForAnalyze);

            var result = await _statisticService.ProcessMantel(new MantelRequest()
            {
                BaseMatrix = groupAnalyze.DistanceMatrix,
                BaseOrganism = closestGene.organism_name,
                NewOrganism = candidateGene.organism_name,
                DerivedMatrix = derivedMatrix.Matrix
            });
            SetMantelCandidateResult(result, groupAnalyze);
            return result;
        }

        private void SetMantelCandidateResult(MantelResponse mantelResponse, OrthoGroupAnalyze groupAnalyze) 
        {
            if (mantelResponse.CorelationCoeficient < groupAnalyze.MantelOrthoGroupResult.PearsonLeftCoeficient) mantelResponse.Response = TestResponseEnum.LessThanLeftBorder;
            else
            {
                if (mantelResponse.CorelationCoeficient < groupAnalyze.MantelOrthoGroupResult.PearsonRightCoeficient) mantelResponse.Response = TestResponseEnum.Success;
                else
                {
                    mantelResponse.Response = TestResponseEnum.MoreThanRightBorder;
                }
            }
        }

        private void SwapGeneRecord(OrthoGroupForAnalyze groupForAnalyze, GeneRecord geneRecord, GeneRecord candidateRecord) 
        {
            int index = groupForAnalyze.GeneRecords.IndexOf(geneRecord);
            if (index != -1) 
            {
                groupForAnalyze.GeneRecords[index] = candidateRecord;
            }
        }
    }
}
