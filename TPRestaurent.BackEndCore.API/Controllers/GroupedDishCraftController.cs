using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

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
        public async Task<AppActionResult> GetAllGroupedDish()
        {
            return await _service.GetAllGroupedDish();
        }

        [HttpGet("get-grouped-dish-by-id/{id}")]
        public async Task<AppActionResult> GetGroupedDishById(Guid id)
        {
            return await _service.GetGroupedDishById(id);
        }

        [HttpPost("add-grouped-dish")]
        public async Task InsertGroupedDish()
        {
            await _service.InsertGroupedDish();
        }
    }
}
