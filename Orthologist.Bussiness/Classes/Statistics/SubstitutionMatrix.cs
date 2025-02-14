namespace Orthologist.Bussiness.Classes
{
    public class SubstitutionMatrix
    {
        public Dictionary<(char, char), int> Matrix { get; set; } = new Dictionary<(char, char), int>();
        public int GapPenalty { get; set; }
        public int GapExtensionPenalty { get; set; }
    }
}
