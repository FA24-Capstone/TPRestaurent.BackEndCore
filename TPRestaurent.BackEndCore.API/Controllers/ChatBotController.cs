using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("chatbot")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private IChatBotService _service;
        public ChatBotController(IChatBotService service)
        {
            _service = service;
        }

        [HttpPost("ai-response")]
        public async Task<AppActionResult> ResponseCustomer(ChatbotRequestDto dto)
        {
            return await _service.ResponseCustomer(dto);             
        }
    }
}
