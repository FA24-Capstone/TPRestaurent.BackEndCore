using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("reservation")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;
        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet("get-all-reservation/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllReservaton(int? time, Domain.Enums.ReservationStatus status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllReservation(time, status,pageNumber, pageSize);
        }

        [HttpGet("get-all-reservation-by-account-id/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllReservatonByAccountId(Guid customerInfoId, Domain.Enums.ReservationStatus status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllReservationByAccountId(customerInfoId, status, pageNumber, pageSize);
        }

        [HttpGet("get-all-reservation-by-phone-number/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllReservationByPhoneNumber(string phoneNumber, Domain.Enums.ReservationStatus status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllReservationByPhoneNumber(phoneNumber, status ,pageNumber, pageSize);
        }

        [HttpGet("get-reservation-detail/{reservationId}")]
        public async Task<AppActionResult> GetAllReservationDetail(Guid reservationId)
        {
            return await _service.GetAllReservationDetail(reservationId);
        }

        [HttpPost("create-reservation")]
        public async Task<AppActionResult> AddReservation([FromBody] ReservationDto dto)
        {
            return await _service.AddReservation(dto);
        }

        [HttpPut("update-reservation")]
        public async Task<AppActionResult> UpdateReservation([FromBody] UpdateReservationDto dto)
        {
            return await _service.UpdateReservation(dto);
        }

        [HttpPost("suggest-table")]
        public async Task<AppActionResult> SuggestTable([FromBody] SuggestTableDto dto)
        {
            return await _service.SuggestTable(dto);
        }

        [HttpPost("calculate-deposit")]
        public async Task<AppActionResult> CalculateDeposit([FromBody] ReservationDto dto)
        {
            return await _service.CalculateDeposit(dto);
        }

        [HttpPost("update-reservation-status/{reservationId}/{status}")]
        public async Task<AppActionResult> UpdateReservationStatus(Guid reservationId, Domain.Enums.ReservationStatus status)
        {
            return await _service.UpdateReservationStatus(reservationId, status);
        }

        [HttpGet("get-table-reservation-with-time")]
        public async Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time)
        {
            return await _service.GetTableReservationWithTime(tableId, time);
        }

    }
}
