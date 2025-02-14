using Orthologist.Bussiness.Classes.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Classes
{
    public  class OrthoGroupAnalyze : BaseDatabaseObject
    {
        public OrthoGroupAnalyze()
        {

        }

        public OrthoGroupAnalyze(OrthoGroupForAnalyze groupForAnalyze)
        {
            Id = groupForAnalyze.Id;
            Name = groupForAnalyze.Name;
            TaxonId = groupForAnalyze.TaxonId;
            TaxonName = groupForAnalyze.TaxonName;
            TaxonRank = groupForAnalyze.TaxonRank;
            GeneRecordCount = groupForAnalyze.GeneRecords.Count;
            OrganismCount = groupForAnalyze.Organisms.Count;
            OrthoGroupName = groupForAnalyze.OrthogroupsNames.First();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public PermanovaOrthoGroupResult PermanovaOrthoGroupResult { get; set; }
        public MantelOrthoGroupResult MantelOrthoGroupResult { get; set; }
        public string OrthoGroupForAnalyzeId { get; set; }
        public string OrthoGroupName { get; set; }
        public string PhyloTree { get; set; }
        public double[,] DistanceMatrix { get; set; }
        public double[,] ZScoreMatrix { get; set; }
        public string[] AllignedSequences { get; set; }
        public string TaxonId { get; set; }
        public string TaxonName { get; set; }
        public string TaxonRank { get; set; }
        public int GeneRecordCount { get; set; }
        public int OrganismCount { get; set; }

        public static string ConvertMatrixToJsonString(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            StringBuilder sb = new StringBuilder();

            sb.Append("[\n");
            for (int i = 0; i < rows; i++)
            {
                sb.Append("  [");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(matrix[i, j].ToString("F3"));
                    if (j < cols - 1) sb.Append(", ");
                }
                sb.Append("]");
                if (i < rows - 1) sb.Append(",\n");
            }
            sb.Append("\n]");

            return sb.ToString();
        }
    }
}
