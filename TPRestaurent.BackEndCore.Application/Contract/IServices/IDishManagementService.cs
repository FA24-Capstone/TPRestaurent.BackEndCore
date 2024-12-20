using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDishManagementService
    {
        public Task<AppActionResult> UpdateDishQuantity(List<UpdateDishQuantityRequest> dto);

        public Task<AppActionResult> LoadDishRequireManualInput();

        public Task<double> CalculatePreparationTime(List<CalculatePreparationTime> dto);

        public Task UpdateComboAvailability();

        public Task UpdateDishAvailability(List<Guid> dishSizeDetailIds = null);

        public Task<AppActionResult> GetDishWithTag(List<string> tags, int batchSize, decimal? low, decimal? high);
    }
}