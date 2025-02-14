using Orthologist.Bussiness.Classes.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Web.Models
{
    public class MantelOrthoGroupResultDto
    {
        public MantelOrthoGroupResultDto(MantelOrthoGroupResult mantelResult)
        {
            if (mantelResult != null)
            {
                PearsonLeftCoeficient = mantelResult.PearsonLeftCoeficient;
                PearsonRightCoeficient = mantelResult.PearsonRightCoeficient;
                PValueLeft = mantelResult.PValueLeft;
                PValueRight = mantelResult.PValueRight;
            }
        }

        public double PearsonLeftCoeficient {  get; set; }
        public double PearsonRightCoeficient { get; set; }
        public double PValueLeft { get; set; }
        public double PValueRight { get; set; }
    }
}
