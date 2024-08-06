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

        [HttpGet("get-all-reservation")]
        public async Task<AppActionResult> GetAllReservaton(int? time, Domain.Enums.ReservationStatus status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllReservation(time, status,pageNumber, pageSize);
        }

        [HttpGet("get-all-reservation-by-account-id")]
        public async Task<AppActionResult> GetAllReservatonByAccountId(string accountid, Domain.Enums.ReservationStatus status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllReservationByAccountId(accountid, status, pageNumber, pageSize);
        }

        [HttpGet("get-reservation-detail/{reservationId}")]
        public async Task<AppActionResult> GetAllReservationDetail(Guid reservationId)
        {
            return await _service.GetAllReservationDetail(reservationId);
        }

        [HttpPost("add-reservation")]
        public async Task<AppActionResult> AddReservation([FromBody] ReservationDto dto)
        {
            return await _service.AddReservation(dto);
        }

        [HttpPost("suggest-table")]
        public async Task<AppActionResult> SuggestTable([FromBody] SuggestTableDto dto)
        {
            return await _service.SuggestTable(dto);
        }

        [HttpPost("calculate-deposit")]
        public async Task<AppActionResult> CalculateDeposit(string reservationDishDtos)
        {
            return await _service.CalculateDeposit(reservationDishDtos);
        }
    }
}
