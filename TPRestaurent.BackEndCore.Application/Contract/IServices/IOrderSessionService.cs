using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOrderSessionService
    {
        Task<AppActionResult> GetAllOrderSession(OrderSessionStatus? orderSessionStatus, int pageNumber, int pageSize);

        Task<AppActionResult> GetOrderSessionById(Guid orderSessionId);

        Task<AppActionResult> UpdateOrderSessionStatus(Guid orderSessionId, OrderSessionStatus orderSessionStatus);

        Task<AppActionResult> GetGroupedDish(DateTime?[]? groupeTime);

        Task DeleteOrderSession();

        Task UpdateLateOrderSession();

        Task ClearOrderSessionDaily();
        public Task<AppActionResult> CheckOrderDetail();
    }
}