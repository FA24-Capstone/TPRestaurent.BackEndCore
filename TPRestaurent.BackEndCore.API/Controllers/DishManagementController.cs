using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("dish-management")]
    [ApiController]
    public class DishManagementController : ControllerBase
    {
        private IDishManagementService _service;
        public DishManagementController(IDishManagementService service)
        {
            _service = service;
        }

        [HttpGet("load-dish-require-manual-input")]
        public async Task<AppActionResult> LoadDishRequireManualInput()
        {
            return await _service.LoadDishRequireManualInput();
        }

        [HttpPut("update-dish-quantity")]
        public async Task<AppActionResult> UpdateDishQuantity(List<UpdateDishQuantityRequest> dto)
        {
            return await _service.UpdateDishQuantity(dto);
        }
    }
}
