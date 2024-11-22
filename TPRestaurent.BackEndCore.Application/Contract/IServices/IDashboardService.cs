using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDashboardService
    {
        Task<AppActionResult> GetStatisticReportForNumberReport(DateTime? startDate, DateTime? endDate);

        Task<AppActionResult> GetStatisticReportForDashboardReport(DateTime? startDate, DateTime? endDate);
    }
}