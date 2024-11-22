using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("table")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private ITableService _service;

        public TableController(ITableService service)
        {
            _service = service;
        }

        [HttpGet("get-all-table/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllTable(int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllTable(pageNumber, pageSize);
        }

        [HttpPost("create-table")]
        public async Task<AppActionResult> CreateTable([FromBody] TableDto dto)
        {
            return await _service.CreateTable(dto);
        }

        [HttpPost("update-table")]
        public async Task<AppActionResult> UpdateTable([FromBody] UpdateTableDto dto)
        {
            return await _service.UpdateTable(dto);
        }

        [HttpPost("delete-table/{id}")]
        public async Task<AppActionResult> DeleteTable(Guid id)
        {
            return await _service.DeleteTable(id);
        }

        [HttpPost("find-table")]
        public async Task<AppActionResult> FindTable([FromBody] FindTableDto dto)
        {
            return await _service.FindTable(dto);
        }

        [HttpPost("update-table-coordinate")]
        public async Task<AppActionResult> UpdateTableCoordinates([FromBody] List<TableArrangementResponseItem> request, bool? isForce = false)
        {
            return await _service.UpdateTableCoordinates(request, isForce);
        }

        [HttpGet("get-all-table-rating/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllTableRating(int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllTableRating(pageNumber, pageSize);
        }
    }
}