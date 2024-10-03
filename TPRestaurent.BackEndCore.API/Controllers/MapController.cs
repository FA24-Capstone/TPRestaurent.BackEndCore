using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("map")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private IMapService _service;
        public MapController(IMapService service)
        {
            _service = service;
        }

        [HttpPost("auto-complete")]
        public async Task<AppActionResult> AutoComplete([FromBody]MapAutoCompleteRequestDto dto)
        {
            return await _service.AutoComplete(dto);
        }

        [HttpGet("geo-code")]
        public async Task<AppActionResult> Geocode(string address)
        {
            return await _service.Geocode(address);
        }

        [HttpPost("get-estimate-delivery-response")]
        public async Task<AppActionResult> GetEstimateDeliveryTime([FromBody]GetEstimateTimeRequest dto)
        {
            return await _service.GetEstimateDeliveryResponse(dto.desc, dto.start);
        }

        [HttpGet("get-order-map")]
        public async Task<AppActionResult> GetGoogleMapLink(Guid? orderId)
        {
            return await _service.GetGoogleMapLink(orderId);
        }

    }
}
