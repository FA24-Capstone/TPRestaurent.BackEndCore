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
        Task<AppActionResult> GetTotalCustomer();
        Task<AppActionResult> GetTotalDeliveringOrder();
        Task<AppActionResult> GetTotalReservation();
        Task<AppActionResult> GetTotalChef();
        Task<AppActionResult> GetTotalShipper();
        Task<AppActionResult> GetProfitReport();
        Task<AppActionResult> GetRevenueReportMonthly();
        Task<AppActionResult> GetOrderStatusReport();  
    }
}
