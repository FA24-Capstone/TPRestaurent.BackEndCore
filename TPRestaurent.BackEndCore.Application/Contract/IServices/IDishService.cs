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
        public Task<AppActionResult> GetAllDish(string? keyword, DishItemType? type, int pageNumber, int pageSize, int? startPrice, int? endPrice);
        public Task<AppActionResult> GetDishById(Guid dishId);
        public Task<AppActionResult> CreateDish(DishDto dto);
        public Task<AppActionResult> UpdateDish(UpdateDishRequestDto dto);
        public Task<AppActionResult> UpdateDishImage(UpdateDishImageRequest dto);
        public Task<AppActionResult> DeleteDish(Guid dishId);
        public Task<AppActionResult> GetAllDishType(int pageNumber, int pagesize);
        public Task<AppActionResult> GetAllDishTag(int pageNumber, int pagesize);
        public Task<AppActionResult> GetAllDishSize(int pageNumber, int pagesize);
        public Task<AppActionResult> UpdateInactiveDish(Guid dishId);
        public Task<AppActionResult> InsertDishTag();
        public Task AutoRefillDish();
    }
}
