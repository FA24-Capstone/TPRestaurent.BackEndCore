﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private  IDishService _dishService;
        public DishController(IDishService dishService)
        {
            _dishService = dishService; 
        }

        [HttpGet("get-all-dish/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDish(string? keyword, DishItemType type, int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDish(keyword, type, pageNumber, pageSize);
        }


        [HttpGet("get-dish-by-id/{dishId}")]
        public async Task<AppActionResult> GetDishbyId(Guid dishId)
        {
            return await _dishService.GetDishById(dishId);  
        }


        [HttpPost("create-dish")]
        public async Task<AppActionResult> CreateDish([FromForm] DishDto dishDto)
        {
            return await _dishService.CreateDish(dishDto);  
        }

        [HttpDelete("delete-dish")]
        public async Task<AppActionResult> DeleteDish(Guid dishId)
        {
            return await _dishService.DeleteDish(dishId);
        }

        [HttpPut("update-dish")]
        public async Task<AppActionResult> UpdateDish([FromForm] UpdateDishRequestDto dto)
        {
            return await _dishService.UpdateDish(dto);
        }
        [HttpGet("get-all-dish-type/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDishType(int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDishType(pageNumber, pageSize);
        }

        [HttpPut("update-inactive-dish")]
        public async Task<AppActionResult> UpdateInactiveADish(Guid dishId)
        {
            return await _dishService.UpdateInactiveADish(dishId);
        }
    }
}
