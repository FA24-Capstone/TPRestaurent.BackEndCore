using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("statistic")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        public IDashboardService _dashboardService;

        public StatisticController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("get-statistic-report-for-number-report")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        [CacheAttribute(259200)]
        public async Task<AppActionResult> GetStatisticReportForNumberReport(DateTime startDate, DateTime endDate)
        {
            return await _dashboardService.GetStatisticReportForNumberReport(startDate, endDate);
        }

        [HttpGet("get-statistic-report-for-dashboard-report")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        [CacheAttribute(259200)]
        public async Task<AppActionResult> GetStatisticReportForDashboardReport(DateTime startDate, DateTime endDate)
        {
            return await _dashboardService.GetStatisticReportForDashboardReport(startDate, endDate);
        }
    }
}