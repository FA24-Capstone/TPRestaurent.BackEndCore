using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("grouped-dish-craft")]
    [ApiController]
    public class GroupedDishCraftController : ControllerBase
    {
        private IGroupedDishCraftService _service;

        public GroupedDishCraftController(IGroupedDishCraftService service)
        {
            _service = service;
        }

        [HttpGet("get-all-grouped-dish")]
        [TokenValidationMiddleware(Permission.CHEF)]
        public async Task<AppActionResult> GetAllGroupedDish()
        {
            return await _service.GetAllGroupedDish();
        }

        [HttpGet("get-grouped-dish-by-id/{id}")]
        [TokenValidationMiddleware(Permission.CHEF)]
        public async Task<AppActionResult> GetGroupedDishById(Guid id, Guid? dishId, bool? isMutual)
        {
            return await _service.GetGroupedDishById(id, dishId, isMutual);
        }

        [HttpPost("update-grouped-dish")]
        //[TokenValidationMiddleware(Permission.CHEF)]
        public async Task<AppActionResult> UpdateForceGroupedDish(List<UpdateGroupedDishDto> dto)
        {
            return await _service.UpdateForceGroupedDish(dto);
        }

        [HttpPost("add-grouped-dish")]
        [TokenValidationMiddleware(Permission.CHEF)]
        public async Task<AppActionResult> InsertGroupedDish()
        {
            return await _service.InsertGroupedDish();
        }

        [HttpDelete("remove-overdue-grouped-dish")]
        [TokenValidationMiddleware(Permission.KITCHEN)]
        public async Task RemoveOverdueGroupedDish()
        {
            await _service.RemoveOverdueGroupedDish();
        }
    }
}