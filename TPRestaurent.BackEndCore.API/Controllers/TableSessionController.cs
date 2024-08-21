using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("table-session")]
    [ApiController]
    public class TableSessionController : ControllerBase
    {
        private ITableSessionService _service;
        public TableSessionController(ITableSessionService service)
        {
            _service = service;
        }

        [HttpPost("add-table-session")]
        public async Task<AppActionResult> AddTableSession(TableSessionDto dto)
        {
            return await _service.AddTableSession(dto);
        }

        [HttpPost("add-new-prelist-order")]
        public async Task<AppActionResult> AddNewPrelistOrder(PrelistOrderDto dto)
        {
            return await _service.AddNewPrelistOrder(dto);
        }

        [HttpPost("update-prelist-order-status")]
        public async Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> list)
        {
            return await _service.UpdatePrelistOrderStatus(list);
        }

        [HttpGet("get-latest-prelist-order")]
        public async Task<AppActionResult> GetLatestPrelistOrder(double? minutes, bool isReadyToServe, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetLatestPrelistOrder(minutes, isReadyToServe, pageNumber, pageSize);
        }

        [HttpGet("get-table-session-by-id/{id}")]
        public async Task<AppActionResult> GetTableSessionById(Guid id)
        {
            return await _service.GetTableSessionById(id);
        }
       
    }
}
