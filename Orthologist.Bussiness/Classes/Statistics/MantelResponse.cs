namespace Orthologist.Bussiness.Classes.Statistics
{
    public class MantelResponse
    {
        public double CorelationCoeficient { get; set; }
        public int PermutationCount { get; set; }
        public double PValue { get; set; }
        public TestResponseEnum Response { get; set; }
    }
}
