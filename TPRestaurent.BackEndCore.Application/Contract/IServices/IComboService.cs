using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IComboService
    {
        Task<AppActionResult> CreateCombo(ComboDto comboDto);
        Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto);
        Task<AppActionResult> DeleteComboById(Guid comboId);
        Task<AppActionResult> GetComboById(Guid comboId);
        public Task<AppActionResult> GetAllCombo(string? keyword, int pageNumber, int pageSize);
    }
}
