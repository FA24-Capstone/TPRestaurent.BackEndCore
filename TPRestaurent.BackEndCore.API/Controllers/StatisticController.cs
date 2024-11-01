using Microsoft.AspNetCore.Http;
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

        [HttpGet("get-total-customer")]
        public async Task<AppActionResult> GetTotalCustomer()
        {
            return await _dashboardService.GetTotalCustomer();  
        }

        [HttpGet("get-total-delivering-order")]
        public async Task<AppActionResult> GetTotalDeliveringOrder()
        {
            return await _dashboardService.GetTotalDeliveringOrder();
        }

        [HttpGet("get-total-chef")]
        public async Task<AppActionResult> GetTotalChef()
        {
            return await _dashboardService.GetTotalChef();
        }

        [HttpGet("get-profit-report")]
        public async Task<AppActionResult> GetProfitReport()
        {
            return await _dashboardService.GetProfitReport();
        }

        [HttpGet("get-total-reservation")]
        public async Task<AppActionResult> GetTotalReservation()
        {
            return await _dashboardService.GetTotalReservation();
        }

        [HttpGet("get-total-shipper")]
        public async Task<AppActionResult> GetTotalShipper()
        {
            return await _dashboardService.GetTotalShipper();
        }
    }
}
