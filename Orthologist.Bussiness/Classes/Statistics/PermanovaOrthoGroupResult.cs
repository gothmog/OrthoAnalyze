using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Classes.Statistics
{
    public class PermanovaOrthoGroupResult : StatOrthoGroupResult
    {
        public double FStatLeft { get; set; }
        public double FStatRight { get; set; }
    }
}
