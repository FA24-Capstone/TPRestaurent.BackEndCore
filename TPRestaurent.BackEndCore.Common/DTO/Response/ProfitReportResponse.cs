using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class ProfitReportResponse
    {
        public double Profit { get; set; }
        public double PercentProfitCompareToYesterday { get; set; }
    }
}
