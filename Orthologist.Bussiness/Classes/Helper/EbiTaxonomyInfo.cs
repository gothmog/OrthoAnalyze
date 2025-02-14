namespace Orthologist.Bussiness.Classes.Helper
{
    public class EbiTaxonomyInfo
    {
        public string TaxId { get; set; }
        public string ScientificName { get; set; }
        public string CommonName { get; set; }
        public bool FormalName { get; set; }
        public string Rank { get; set; }
        public string Division { get; set; }
        public string Lineage { get; set; }
        public IList<string> LineageList { get; set; }
        public string GeneticCode { get; set; }
        public string MitochondrialGeneticCode { get; set; }
        public bool Submittable { get; set; }
        public bool Binomial { get; set; }
        public List<string> Synonyms { get; set; }
    }
}
