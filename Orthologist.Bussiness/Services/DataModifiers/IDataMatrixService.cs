using Orthologist.Bussiness.Classes;
using Orthologist.Bussiness.Classes.Statistics;

namespace Orthologist.Bussiness.Services.DataModifiers
{
    public interface IDataMatrixService
    {
        DistanceMatrix GenerateDistanceMatrix(OrthoGroupForAnalyze orthoGroup, string prefix = "");
    }
}