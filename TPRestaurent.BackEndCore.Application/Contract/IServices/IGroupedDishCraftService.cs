using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IGroupedDishCraftService
    {
        public Task<AppActionResult> GetAllGroupedDish();
        public Task<AppActionResult> GetGroupedDishById(Guid groupedDishId, Guid? dishId, bool? isMutual);
        public Task<AppActionResult> UpdateGroupedDish(List<Guid> OrderDetailIds);
        public Task<AppActionResult> InsertGroupedDish();
        public Task UpdateLateWarningGroupedDish();
        public Task RemoveOverdueGroupedDish();
    }
}
