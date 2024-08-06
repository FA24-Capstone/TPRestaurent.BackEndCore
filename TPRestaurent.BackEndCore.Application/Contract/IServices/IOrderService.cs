using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOrderService
    {
        public Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, int pageNumber, int paeSize);
        public Task<AppActionResult> GetOrderDetail(Guid orderId);
    }
}
