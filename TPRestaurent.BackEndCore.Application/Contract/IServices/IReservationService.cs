﻿using System;
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
        public Task<AppActionResult> GetAvailableTable(DateTime startTime, DateTime? endTime, int? numOfPeople, int pageNumber, int pageSize);
        public Task<AppActionResult> AddReservation(ReservationDto dto);
        public Task<AppActionResult> GetAllReservation(int? time, Domain.Enums.ReservationStatus? status, int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllReservationByAccountId(Guid customerInfoId, Domain.Enums.ReservationStatus? status, int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllReservationDetail(Guid reservationId);
        public Task<AppActionResult> UpdateReservation(UpdateReservationDto dto);
        public Task<AppActionResult> CalculateDeposit(List<ReservationDishDto> reservationDishDtos);
        public Task<AppActionResult> SuggestTable(SuggestTableDto dto);
        public Task<AppActionResult> UpdateReservationStatus(Guid reservationId, Domain.Enums.ReservationStatus status);

    }
}
