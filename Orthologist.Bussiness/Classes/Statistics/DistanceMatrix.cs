namespace Orthologist.Bussiness.Classes.Statistics
{
    public class DistanceMatrix
    {
        public double[,] Matrix { get; set; }
        public double[,] ZscoreMatrix { get; set; }
        public string Name { get; set; }
        public List<GeneRecord> Records { get; set; }
        public string[] AllignedFasta { get; set; }
    }
}
