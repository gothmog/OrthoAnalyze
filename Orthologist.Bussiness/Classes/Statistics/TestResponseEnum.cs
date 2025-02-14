using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Classes.Statistics
{
    public enum TestResponseEnum
    {
        WithoutClassification,
        Success,
        LessThanLeftBorder,
        MoreThanRightBorder
    }
}
