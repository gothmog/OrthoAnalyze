
using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;

namespace Orthologist.Bussiness.Services.OrthoDb
{
    public interface IOrthoGeneService
    {
        Task WriteFastaToOrthoGroups();
        Task LoadAllOrganismsOrthoGroupsWithFasta(string taxonName);
    }
}