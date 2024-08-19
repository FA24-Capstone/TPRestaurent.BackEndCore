using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITableSessionService
    {
        public Task<AppActionResult> AddTableSession(TableSessionDto dto);
        public Task<AppActionResult> AddNewPrelistOrder(PrelistOrderDto dto);
        public Task<AppActionResult> GetTableSessionById(Guid Id);
        public Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> prelistOrderIds);
        public Task<AppActionResult> GetLatestPrelistOrder(double? minute, bool IsReadyToServed, int pageNumber, int pageSize);
    }
}
