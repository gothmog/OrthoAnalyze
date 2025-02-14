using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Services.Database;
using Orthologist.Bussiness.Services.DataModifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Services.DataProviders
{
    public class OrthoGroupService : IOrthoGroupService
    {
        IDatabaseProvider _databaseProvider;
        IOrthoGroupModifierService _groupModifierService;

        public OrthoGroupService(IDatabaseProvider databaseProvider, IOrthoGroupModifierService groupModifierService)
        {
            _databaseProvider = databaseProvider;
            _groupModifierService = groupModifierService;
        }

        public async Task<OrthoGroupForAnalyze> GetOrthoGeneGroupAsync(string groupId)
        {
            return await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
        }

        public async Task<OrthoGroupForAnalyze> GetOrthoGroupMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate)
        {
            var orthoGroupForAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
            if (candidate == null)
            {
                GeneRecord rec = orthoGroupForAnalyze.GeneRecords.FirstOrDefault(x=> x.organism_name == "Pongo pygmaeus");
                orthoGroupForAnalyze.GeneRecords = orthoGroupForAnalyze.GeneRecords.Where(x => x.organism_name != "Pongo pygmaeus").ToList();
                var res = await _groupModifierService.CreateMatrixesForCanidateGeneMantel(orthoGroupForAnalyze, rec, "Homo sapiens");
            }
            else
            {
                var res = await _groupModifierService.CreateMatrixesForCanidateGeneMantel(orthoGroupForAnalyze, candidate, orgToLeft);
            }
            return await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
        }

        public async Task<OrthoGroupForAnalyze> GetOrthoGroupZscoreMatrixComparationAsync(string groupId, string orgToLeft, GeneRecord candidate)
        {
            var orthoGroupForAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
            var res = await _groupModifierService.CreateZscoreMatrixesForCanidateGeneMantel(orthoGroupForAnalyze, candidate, orgToLeft);
            return await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
        }

        public async Task<OrthoGroupAnalyze> GetOrthoGeneGroupAnalyzeAsync(string groupId)
        {
            var orthoGroupAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupAnalyze>(groupId);
            if (orthoGroupAnalyze == null)
            {
                var orthoGroupForAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
                orthoGroupAnalyze = await _groupModifierService.WriteTreeToOrthoGroup(orthoGroupForAnalyze);
            }
            return orthoGroupAnalyze;
        }

        public async Task SetTestGroup(string groupId, string orgToLeft)
        {
            var orthoGroupForAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
            orthoGroupForAnalyze.GeneRecords = orthoGroupForAnalyze.GeneRecords.Where(x=> x.organism_name != orgToLeft).ToList();
            orthoGroupForAnalyze.Name = orthoGroupForAnalyze.Name + "_test";
            orthoGroupForAnalyze.Id = Guid.NewGuid().ToString();
            await _databaseProvider.CreateOrthoObjectAsync(orthoGroupForAnalyze);
        }

        public async Task SetTrasholdGroup(string groupId)
        {
            var orthoGroupForAnalyze = await _databaseProvider.GetOrthoObjectAsync<OrthoGroupForAnalyze>(groupId);
            var orthoGroupAnalyze = await GetOrthoGeneGroupAnalyzeAsync(groupId);
            await _groupModifierService.CreateMantelTresholdForOrthoGroup(orthoGroupForAnalyze, orthoGroupAnalyze, true);
            await _databaseProvider.UpdateOrthoObjectAsync(orthoGroupAnalyze);
        }
    }
}
