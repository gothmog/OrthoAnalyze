using Nest;

namespace Orthologist.Bussiness.Classes
{
    public class OrthoAnalyze : BaseDatabaseObject
    {
        public List<Organism> Organisms { get; set; } = new List<Organism>();
        public List<OrthoGene> OrthoGenes { get; set; } = new List<OrthoGene>();
        public string User { get; set; }
        public string Note { get; set; }
    }
}
