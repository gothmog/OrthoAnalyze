namespace Orthologist.Web.Models
{
    public class OrthoGroupResult
    {
        public IList<OrthoGroupDto> Groups { get; set; }
        public long TotalCount { get; set; }
    }
}
