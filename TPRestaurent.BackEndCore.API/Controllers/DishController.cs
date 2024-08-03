using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private  IDishService _dishService;
        public DishController(IDishService dishService)
        {
            _dishService = dishService; 
        }

        [HttpGet("get-all-dish/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDish(string? keyword, int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDish(keyword, pageNumber, pageSize);
        }

        [HttpGet("get-dish-by-id/{dishId}")]
        public async Task<AppActionResult> GetDishbyId(Guid dishId)
        {
            return await _dishService.GetDishById(dishId);  
        }


        [HttpPost("create-dish")]
        public async Task<AppActionResult> CreateDish(DishDto dishDto)
        {
            return await _dishService.CreateDish(dishDto);  
        }

        [HttpDelete("delete-dish")]
        public async Task<AppActionResult> DeleteDish(Guid dishId)
        {
            return await _dishService.DeleteDish(dishId);
        }

        [HttpPut("update-dish")]
        public async Task<AppActionResult> UpdateDish(UpdateDishRequestDto dto)
        {
            return await _dishService.UpdateDish(dto);
        }

    }
}
