﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

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
        public async Task<AppActionResult> GetStatisticReportForNumberReport(DateTime startDate, DateTime endDate)
        {
            return await _dashboardService.GetStatisticReportForNumberReport(startDate, endDate);  
        }


        [HttpGet("get-statistic-report-for-dashboard-report")]
        public async Task<AppActionResult> GetStatisticReportForDashboardReport(DateTime startDate, DateTime endDate)
        {
            return await _dashboardService.GetStatisticReportForDashboardReport(startDate, endDate);
        }

     
    }
}
