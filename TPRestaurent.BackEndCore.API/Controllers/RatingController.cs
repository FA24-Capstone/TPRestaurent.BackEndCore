using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("rating")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [HttpGet("get-all-rating-of-dish/{dishId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllRatingOfDish(Guid dishId, RatingPoint? ratingPoint, int pageNumber = 1, int pageSize = 10)
        {
            return await _ratingService.GetAllRatingOfDish(dishId, ratingPoint, pageNumber, pageSize);
        }

        [HttpGet("get-rating-by-id/{ratingId}")]
        public async Task<AppActionResult> GetRatingById(Guid ratingId)
        {
            return await _ratingService.GetRatingById(ratingId);
        }

        [HttpPost("create-rating")]
        [TokenValidationMiddleware(Permission.CUSTOMER)]
        public async Task<AppActionResult> CreateRating([FromForm] CreateRatingRequestDto createRatingRequestDto)
        {
            return await _ratingService.CreateRating(createRatingRequestDto);
        }

        [HttpPut("update-rating")]
        [TokenValidationMiddleware(Permission.CUSTOMER)]
        public async Task<AppActionResult> UpdateRating([FromForm] UpdateRatingRequestDto updateRatingRequestDto)
        {
            return await _ratingService.UpdateRating(updateRatingRequestDto);
        }

        [HttpDelete("delete-rating")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> DeleteRating(Guid ratingId)
        {
            return await _ratingService.DeleteRating(ratingId);
        }
    }
}