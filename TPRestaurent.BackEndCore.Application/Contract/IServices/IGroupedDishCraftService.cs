using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

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
        public Task<AppActionResult> UpdateForceGroupedDish(List<UpdateGroupedDishDto> dto);
    }
}