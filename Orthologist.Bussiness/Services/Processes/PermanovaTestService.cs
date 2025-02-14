using Orthologist.Bussiness.Classes.Statistics;
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Services.DataModifiers;
using Orthologist.Bussiness.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Services.Processes
{
    public class PermanovaTestService : BaseTestService, IPermanovaTestService
    {
        IOrthoGroupModifierService _orthoGroupModifierService;

        public PermanovaTestService(IDataMatrixService dataMatrixService, IRStatisticService statisticService, IOrthoGroupModifierService orthoGroupModifierService) : base(dataMatrixService, statisticService)
        {
            _orthoGroupModifierService = orthoGroupModifierService;
        }

        public async Task<PermanovaResponse> ProcessTestWithCandidate(OrthoGroupForAnalyze groupForAnalyze, OrthoGroupAnalyze groupAnalyze, GeneRecord candidateGene)
        {
            if (groupAnalyze.PermanovaOrthoGroupResult == null)
            {
                await _orthoGroupModifierService.CreatePermanovaTresholdForOrthoGroup(groupForAnalyze, groupAnalyze);
            }
            IList<MatrixRequest> matrixes = new List<MatrixRequest>();
            IList<GeneRecord> originalGeneRecords = new List<GeneRecord>();
            groupForAnalyze.GeneRecords.CopyTo(originalGeneRecords.ToArray(), 0);
            //Vytváření matic, které jsou odvozeny od kandidátního genu nahrazením všech genů
            foreach (var gene in originalGeneRecords)
            {
                if (gene.Fasta != candidateGene.Fasta)
                {
                    SwapGeneRecord(groupForAnalyze, gene, candidateGene);
                    var derivedMatrix = _dataMatrixService.GenerateDistanceMatrix(groupForAnalyze);
                    matrixes.Add(new MatrixRequest() { BaseOrganism = gene.organism_name, NewOrganism = candidateGene.organism_name, DerivedMatrix = derivedMatrix.Matrix });
                }
            }

            var result = await _statisticService.ProcessPermanova(new PermanovaRequest()
            {
                Matrixes = matrixes
            });
            SetPermanovaCandidateResult(result, groupAnalyze);
            return result;
        }

        private void SetPermanovaCandidateResult(PermanovaResponse permanovaResponse, OrthoGroupAnalyze groupAnalyze)
        {
            if (permanovaResponse.Fstat < groupAnalyze.PermanovaOrthoGroupResult.FStatLeft) permanovaResponse.Response = TestResponseEnum.LessThanLeftBorder;
            else
            {
                if (permanovaResponse.Fstat < groupAnalyze.PermanovaOrthoGroupResult.FStatRight) permanovaResponse.Response = TestResponseEnum.Success;
                else
                {
                    permanovaResponse.Response = TestResponseEnum.MoreThanRightBorder;
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
