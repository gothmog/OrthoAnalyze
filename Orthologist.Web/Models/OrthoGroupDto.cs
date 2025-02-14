using Orthologist.Bussiness.Classes;

namespace Orthologist.Web.Models
{
    public class OrthoGroupDto
    {
        public OrthoGroupDto() { }
        public OrthoGroupDto(OrthoGroupForAnalyze orthoGroup) 
        {
            Id = orthoGroup.Id;
            Name = orthoGroup.Name;
            OrgGroupName = orthoGroup.OrthogroupsNames.First();
            Organisms = orthoGroup.Organisms.Select(x=> new OrganismDto() { OrganismName = x}).ToList();
            Count = orthoGroup.GeneRecords.Count();
            FamilyName = orthoGroup.TaxonName;
            GeneRecords = orthoGroup.GeneRecords.Select(x=> new GeneRecordDto() { Organism = x.organism_name, Fasta = x.Fasta }).ToList();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public IList<OrganismDto> Organisms { get; set; }
        public IList<GeneRecordDto> GeneRecords { get; set; }
        public string OrgGroupName { get; set; }
        public int Count { get; set; }
        public string FamilyId { get; set; }
        public string FamilyName { get; set; }
        public string GeneDescriptions { get; set; }
    }
}
