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
        //public Task<AppActionResult> GetTableById(Guid TableId);
        public Task<AppActionResult> CreateTable(TableDto dto);
        public Task<AppActionResult> UpdateTableCoordinates(List<TableArrangementResponseItem> request, bool? isForce = false);
        public Task<AppActionResult> FindTable(FindTableDto dto);

        public Task<AppActionResult> GetAllTableRating(int pageNumber, int pageSize);
    }
}
