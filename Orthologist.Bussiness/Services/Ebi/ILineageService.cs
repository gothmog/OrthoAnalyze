
using Orthologist.Bussiness.Classes;

namespace Orthologist.Bussiness.Services.Ebi
{
    public interface ILineageService
    {
        Task<Organism> GetInfosForOrganism(string organismName);
    }
}