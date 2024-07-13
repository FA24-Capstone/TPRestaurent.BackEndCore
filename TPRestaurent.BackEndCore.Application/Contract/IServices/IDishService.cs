﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDishService
    {
        public Task<AppActionResult> GetAllDish(string? keyword, int pageNumber, int pageSize);
        public Task<AppActionResult> GetDishById(Guid dishId);
        public Task<AppActionResult> CreateDish(DishDto dto);
        public Task<AppActionResult> UpdateDish(Guid dishId, DishDto dto);
        public Task<AppActionResult> DeleteDish(Guid dishId);
    }
}
