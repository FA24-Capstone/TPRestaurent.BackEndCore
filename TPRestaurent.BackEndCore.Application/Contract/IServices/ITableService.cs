using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITableService
    {
        public Task<AppActionResult> GetAllTable(int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllAvailableTable(int pageNumber, int pageSize);
        public Task<AppActionResult> GetTableById(Guid TableId);
        public Task<AppActionResult> CreateTable(TableDto dto);
        public Task<AppActionResult> UpdateTable(Guid TableId, TableDto dto);
    }
}
