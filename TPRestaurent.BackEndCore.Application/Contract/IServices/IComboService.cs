using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IComboService
    {
        public Task<AppActionResult> GetAllCombo(string? keyword, int pageNumber, int pageSize);
        public Task<AppActionResult> GetComboById(Guid ComboId);
        public Task<AppActionResult> CreateCombo(ComboDto dto);
        public Task<AppActionResult> UpdateCombo(Guid ComboId, ComboDto dto);
    }
}
