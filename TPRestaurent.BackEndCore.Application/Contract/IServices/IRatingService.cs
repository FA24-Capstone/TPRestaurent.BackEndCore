using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IRatingService
    {
        Task<AppActionResult> GetAllRatingOfDish(Guid dishId, RatingPoint? ratingPoint , int pageNumber, int pageSize);
        Task<AppActionResult> GetRatingById(Guid ratingId);
        Task<AppActionResult> CreateRating(CreateRatingRequestDto createRatingRequestDto);
        Task<AppActionResult> UpdateRating(UpdateRatingRequestDto updateRatingRequestDto);
        Task<AppActionResult> DeleteRating(Guid ratingId);
    }
}
