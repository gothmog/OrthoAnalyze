using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Web.Models
{
    public  class OrthoGroupAnalyzeDto
    {
        public OrthoGroupAnalyzeDto(OrthoGroupAnalyze group)
        {
            PermanovaOrthoGroupResult = new PermanovaOrthoGroupResultDto(group.PermanovaOrthoGroupResult);
            MantelOrthoGroupResult = new MantelOrthoGroupResultDto(group.MantelOrthoGroupResult);
            OrthoGroupForAnalyzeId = group.OrthoGroupForAnalyzeId;
            OrthoGroupName = group.OrthoGroupName;
            PhyloTree = group.PhyloTree;
            DistanceMatrix = group.DistanceMatrix;
            TaxonId = group.TaxonId;
            TaxonName = group.TaxonName;
            TaxonRank = group.TaxonRank;
            GeneRecordCount = group.GeneRecordCount;
            OrganismCount = group.OrganismCount;
        }

        public PermanovaOrthoGroupResultDto PermanovaOrthoGroupResult { get; set; }
        public MantelOrthoGroupResultDto MantelOrthoGroupResult { get; set; }
        public string OrthoGroupForAnalyzeId { get; set; }
        public string OrthoGroupName { get; set; }
        public string PhyloTree { get; set; }
        public double[,] DistanceMatrix { get; set; }
        public string TaxonId { get; set; }
        public string TaxonName { get; set; }
        public string TaxonRank { get; set; }
        public int GeneRecordCount { get; set; }
        public int OrganismCount { get; set; }
    }
}
