using Nest;

namespace Orthologist.Bussiness.Classes
{
    public class Organism : BaseDatabaseObject
    {
        public string TaxId { get; set; }
        public string CommonName { get; set; }
        public bool FormalName { get; set; }
        public string Rank { get; set; }
        public string OrganismDescription { get; set; }
        public IList<string> Lineage { get; set; }
        public bool InOrthoDb { get; set; }
        public bool IsMain { get; set; } = false;
        public IList<OrthoGene> OrthoGenes { get; set; } = new List<OrthoGene>();
    }
}
