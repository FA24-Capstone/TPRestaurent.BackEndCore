using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
