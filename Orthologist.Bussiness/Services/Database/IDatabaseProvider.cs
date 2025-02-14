using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Helper;

namespace Orthologist.Bussiness.Services.Database
{
    public interface IDatabaseProvider
    {
        Task<bool> CreateOrthoObjectAsync<T>(T obj) where T : BaseDatabaseObject;
        Task<bool> DeleteOrthoObjectAsync<T>(string id) where T : BaseDatabaseObject;
        Task<T> GetOrthoObjectAsync<T>(string id) where T : BaseDatabaseObject;
        T GetOrthoObject<T>(string id) where T : BaseDatabaseObject;
        Task<List<T>> GetAllOrthoObjectsAsync<T>() where T : BaseDatabaseObject;
        Task<bool> UpdateOrthoObjectAsync<T>(T obj) where T : BaseDatabaseObject;
        Task<(List<OrthoGroupForAnalyze>, long)> GetOrthogroupsByOrganisms(IList<string> organismNames, int page, int pageNum, IList<FilterItem> filters = null);
        Task<(List<OrthoGroupForAnalyze>, long)> GetOrthogroupsByOrganisms(IList<string> organismNames, int page, int pageNum);
        Task<List<OrthoGroupForAnalyze>> GetOrthoGroupForFastaUpdate(int count);
        Task<TaxTree> LoadTree();
        Task<bool> SaveTree(TaxTree tree);
    }
}