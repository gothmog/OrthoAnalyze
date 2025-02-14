using Orthologist.Bussiness.Classes.Statistics;

namespace Orthologist.Bussiness.Services.Statistics
{
    public interface IRStatisticService
    {
        Task<MantelResponse> ProcessMantel(MantelRequest request);
        Task<PermanovaResponse> ProcessPermanova(PermanovaRequest request);
    }
}