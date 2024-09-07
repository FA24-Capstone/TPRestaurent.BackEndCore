using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("reservation-request")]
    [ApiController]
    public class ReservationRequestController : ControllerBase
    {
        private IReservationRequestService _service;
        public ReservationRequestController(IReservationRequestService service) 
        { 
            _service = service;
        }

        //[HttpGet("get-all-reservation-request")]
        //public async Task<AppActionResult> GetAllReservationRequest(Domain.Enums.ReservationRequestStatus status, int pageNumber = 1, int pageSize = 10)
        //{
        //    return await _service.GetAllReservationRequest(status, pageNumber, pageSize);
        //}

        //[HttpPost("create-reservation-request")]
        //public async Task<AppActionResult> CreateReservationRequest(ReservationRequestDto dto)
        //{
        //    return await _service.CreateReservationRequest(dto);
        //}

        //[HttpPut("create-reservation-request")]
        //public async Task<AppActionResult> UpdateStatus(Guid reservationRequestId, Domain.Enums.ReservationRequestStatus status)
        //{
        //    return await _service.UpdateStatus(reservationRequestId, status);
        //}

    }
}
