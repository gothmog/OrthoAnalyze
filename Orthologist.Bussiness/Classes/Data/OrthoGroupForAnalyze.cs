namespace Orthologist.Bussiness.Classes
{
    public class OrthoGroupForAnalyze : BaseDatabaseObject
    {
        public IList<GeneRecord> GeneRecords { get; set; }
        public string TaxonId { get; set; }
        public string TaxonName { get; set; }
        public string TaxonRank { get; set; }
        public IList<string> Organisms { get; set; }
        public IList<string> OrthogroupsNames { get; set; }
    }

    public class GeneRecord
    {
        public string pub_og_id { get; set; }
        public string og_name { get; set; }
        public string level_taxid { get; set; }
        public string organism_taxid { get; set; }
        public string organism_name { get; set; }
        public string pub_gene_id { get; set; }
        public string description { get; set; }
        public string Fasta { get; set; }
    }
}
