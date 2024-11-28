using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IMapService
    {
        public Task<double[]> GetPlaceById(string id);

        public Task<AppActionResult> Geocode(string address);

        public Task<AppActionResult> AutoComplete(MapAutoCompleteRequestDto dto);

        public Task<AppActionResult> GetEstimateDeliveryResponse(double[] endCoordinate, double[]? startCoordinate);
        public Task<AppActionResult> CheckValidShipperDistance(double[] shipperLocation, double[] deliveryDistance);

        public Task<AppActionResult> GetOptimalPath(List<Guid> orderIds);

        public Task<AppActionResult> GetGoogleMapLink(Guid? addressId);

        public Task<AppActionResult> CheckDistanceForChatBot(string address, string? customerName);
    }
}