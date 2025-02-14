namespace Orthologist.Bussiness.Classes.Statistics
{
    public class PermanovaResponse
    {
        public double Fstat { get; set; }
        public double PValue { get; set; }
        public TestResponseEnum Response { get; set; }
    }
}
