using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDashboardService
    {
        Task<AppActionResult> GetStatisticReportForNumberReport(DateTime? startDate, DateTime? endDate);
        Task<AppActionResult> GetStatisticReportForDashboardReport(DateTime? startDate, DateTime? endDate);

    }
}
