
namespace Orthologist.Web.Services.Bussiness
{
    public interface ITreeService
    {
        IList<string> LoadSpecies();
        IList<string> LoadTaxons();
        Task EnsureInitializedAsync();
        Task<string> LoadSubTree(string nodeId);
        Task<string> GetJsonForGraph();
    }
}