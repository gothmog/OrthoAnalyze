using Nest;

namespace Orthologist.Bussiness.Classes
{
    public class BaseDatabaseObject
    {
        [Keyword]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Created {  get; set; }
        public DateTime ModificationDate { get; set; }
        public string Name { get; set; }  
    }
}
