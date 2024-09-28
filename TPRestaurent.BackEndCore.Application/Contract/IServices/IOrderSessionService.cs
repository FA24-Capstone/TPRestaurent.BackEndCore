using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOrderSessionService
    {
        Task<AppActionResult> GetAllOrderSession(OrderSessionStatus? orderSessionStatus, int pageNumber, int pageSize);
        Task<AppActionResult> GetOrderSessionById(Guid orderSessionId);
        Task<AppActionResult> UpdateOrderSessionStatus(Guid orderSessionId, OrderSessionStatus orderSessionStatus);
        Task<AppActionResult> GetGroupedDish();
        Task DeleteOrderSession();
    }
}
