using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/combo")]
    [ApiController]
    public class ComboController : ControllerBase
    {
        private IComboService _comboService;
        public ComboController(IComboService comboService)
        {
            _comboService = comboService;   
        }

        [HttpGet("get-all-combo/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllCombo(string? keyword, int pageNumber = 1, int pageSize = 10)
        {
            return await _comboService.GetAllCombo(keyword, pageNumber, pageSize);
        }

        [HttpGet("get-combo-by-id/{combiId}")]
        public async Task<AppActionResult> GetComboById(Guid comboId)
        {
            return await _comboService.GetComboById(comboId);   
        }

        [HttpPost("create-combo")]
        public async Task<AppActionResult> CreateCombo(ComboDto comboDto)
        {
            return await _comboService.CreateCombo(comboDto);     
        }

        [HttpDelete("delete-combo-by-id")]
        public async Task<AppActionResult> DeleteComboById(Guid comboId)
        {
            return await _comboService.DeleteComboById(comboId);        
        }

        [HttpPut("update-combo")]
        public async Task<AppActionResult> UpdateCombo(UpdateComboDto comboDto)
        {
            return await _comboService.UpdateCombo(comboDto);   
        }
    }
}
