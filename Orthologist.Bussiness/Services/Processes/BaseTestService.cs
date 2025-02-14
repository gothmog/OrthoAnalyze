using Orthologist.Bussiness.Services.DataModifiers;
using Orthologist.Bussiness.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orthologist.Bussiness.Services.Processes
{
    public class BaseTestService
    {
        protected IDataMatrixService _dataMatrixService;
        protected IRStatisticService _statisticService;
        public BaseTestService(IDataMatrixService dataMatrixService, IRStatisticService statisticService) 
        { 
            _dataMatrixService = dataMatrixService;
            _statisticService = statisticService;
        }
    }
}
