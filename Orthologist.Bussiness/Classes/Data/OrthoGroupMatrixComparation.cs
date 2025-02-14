using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Classes
{
    public class OrthoGroupMatrixComparation : BaseDatabaseObject
    {
        public string OrganismForLeft {  get; set; }
        public string OrganismForReplace { get; set; }
        public double[,] BaseDistanceMatrix { get; set; }
        public double[,] DerivedDistanceMatrix { get; set; }
    }
}
