using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Classes.Statistics
{
    public class MantelOrthoGroupResult : StatOrthoGroupResult
    {
        public double PearsonLeftCoeficient {  get; set; }
        public double PearsonRightCoeficient { get; set; }
    }
}
