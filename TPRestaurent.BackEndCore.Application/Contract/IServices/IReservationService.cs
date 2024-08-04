using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IReservationService
    {
        public Task<AppActionResult> GetAvailableTable(DateTime? startTime, DateTime? endTime, int? numOfPeople, int pageNumber, int pageSize);
        public Task<AppActionResult> AddReservation(ReservationDto dto);
        public Task<AppActionResult> UpdateReservation(ReservationDto dto);
        public Task<AppActionResult> RemoveReservation(Guid reservationId);
        public Task<AppActionResult> CalculateDeposit(string ReservationDishDtos);
        public Task<AppActionResult> SuggestTable(SuggestTableDto dto);

    }
}
