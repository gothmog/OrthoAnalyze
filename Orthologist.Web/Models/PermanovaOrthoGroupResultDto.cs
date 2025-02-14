using Orthologist.Bussiness.Classes.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Web.Models
{
    public class PermanovaOrthoGroupResultDto
    {
        public PermanovaOrthoGroupResultDto(PermanovaOrthoGroupResult permanovaResult)
        {
            if (permanovaResult != null)
            {
                FStatLeft = permanovaResult.FStatLeft;
                FStatRight = permanovaResult.FStatRight;
                PValueLeft = permanovaResult.PValueLeft;
                PValueRight = permanovaResult.PValueRight;
            }
        }

        public double FStatLeft { get; set; }
        public double FStatRight { get; set; }
        public double PValueLeft { get; set; }
        public double PValueRight { get; set; }
    }
}
