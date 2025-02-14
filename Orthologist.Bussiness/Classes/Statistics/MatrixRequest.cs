namespace Orthologist.Bussiness.Classes.Statistics
{
    public class MatrixRequest
    {
        public double[,] DerivedMatrix { get; set; }
        public string BaseOrganism { get; set; }
        public string NewOrganism { get; set; }
    }
}
