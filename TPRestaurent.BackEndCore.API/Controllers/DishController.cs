﻿using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private IDishService _dishService;

        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }

        [HttpGet("get-all-dish/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDish(string? keyword, int? startPrice, int? endPrice, DishItemType type, int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDish(keyword, type, pageNumber, pageSize, startPrice, endPrice);
        }

        [HttpGet("get-dish-by-id/{dishId}")]
        public async Task<AppActionResult> GetDishbyId(Guid dishId)
        {
            return await _dishService.GetDishById(dishId);
        }

        [HttpPost("create-dish")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> CreateDish([FromForm] DishDto dishDto)
        {
            return await _dishService.CreateDish(dishDto);
        }

        [HttpPost("delete-dish")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> DeleteDish(Guid dishId)
        {
            return await _dishService.DeleteDish(dishId);
        }

        [HttpPut("update-dish")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> UpdateDish([FromBody] UpdateDishRequestDto dto)
        {
            return await _dishService.UpdateDish(dto);
        }

        [HttpGet("get-all-dish-type/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDishType(int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDishType(pageNumber, pageSize);
        }

        [HttpGet("get-all-dish-tag/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDishTag(int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDishTag(pageNumber, pageSize);
        }

        [HttpGet("get-all-dish-size/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllDishSize(int pageNumber = 1, int pageSize = 10)
        {
            return await _dishService.GetAllDishSize(pageNumber, pageSize);
        }

        [HttpPut("update-inactive-dish")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> UpdateInactiveADish(Guid dishId)
        {
            return await _dishService.UpdateInactiveDish(dishId);
        }

        [HttpPut("update-dish-image")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> UpdateDishImage([FromForm] UpdateDishImageRequest imageRequest)
        {
            return await _dishService.UpdateDishImage(imageRequest);
        }

        //[HttpPost("upload-dish-tag")]
        //public async Task<AppActionResult> InsertDishTag()
        //{
        //    return await _dishService.InsertDishTag();
        //}
    }
}