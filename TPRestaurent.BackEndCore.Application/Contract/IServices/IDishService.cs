using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDishService
    {
        public Task<AppActionResult> GetAllDish(string? keyword, DishItemType? type, int pageNumber, int pageSize);
        public Task<AppActionResult> GetDishById(Guid dishId);
        public Task<AppActionResult> CreateDish(DishDto dto);
        public Task<AppActionResult> UpdateDish(UpdateDishRequestDto dto);
        public Task<AppActionResult> DeleteDish(Guid dishId);
    }
}
