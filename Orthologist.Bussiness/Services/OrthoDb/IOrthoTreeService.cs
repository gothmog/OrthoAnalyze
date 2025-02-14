namespace Orthologist.Bussiness.Services.OrthoDb
{
    public interface IOrthoTreeService
    {
        Task EnsureInitializedAsync();
        Task<string> GetJsonForGraph();
        Task<string> LoadSubTree(string nodeId);
        Task<IList<string>> LoadMembersForFamily(string familyName);
        Task<Dictionary<string, string>> GetAllOrganismForTaxon(string taxonName);
        IList<string> LoadTaxons();
        IList<string> LoadSpecies();
    }
}