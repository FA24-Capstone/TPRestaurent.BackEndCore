using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITableService
    {
        public Task<AppActionResult> GetAllTable(int pageNumber, int pageSize);

        //public Task<AppActionResult> GetTableById(Guid TableId);
        public Task<AppActionResult> CreateTable(TableDto dto);

        public Task<AppActionResult> UpdateTable(UpdateTableDto dto);

        public Task<AppActionResult> UpdateTableCoordinates(List<TableArrangementResponseItem> request, bool? isForce = false);
        public Task<AppActionResult> CheckUpdateTableCoordinates(List<TableArrangementResponseItem> request);

        public Task<AppActionResult> FindTable(FindTableDto dto);

        public Task<AppActionResult> DeleteTable(Guid id);

        public Task<AppActionResult> GetAllTableRating(int pageNumber, int pageSize);
        public Task<AppActionResult> UpdateTableAvailability(List<Guid> tableIds, TableStatus tableStatus);
        public Task<AppActionResult> UpdateTableAvailabilityAfterPayment(Guid orderId, TableStatus tableStatus);
        public Task UpdateTableAvailability();
    }
}