using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class MapService : GenericBackendService, IMapService
    {
        public const string APIKEY = "YfkkZsLrlGAJHZjFaAqXPKoLNXdC32hOlCdJrP3U";
        private IUnitOfWork _unitOfWork;

        public MapService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AutoComplete(MapAutoCompleteRequestDto dto)
        {
            AppActionResult result = new();

            try
            {
                StringBuilder endpoint = new StringBuilder();                   
                endpoint.Append($"https://rsapi.goong.io/Place/AutoComplete?api_key={APIKEY}");
                if(dto.Destination != null)
                {
                    endpoint.Append($"&location={dto.Destination[0]},{dto.Destination[1]}");
                }
                endpoint.Append($"&input={dto.Address}");
                var client = new RestClient();
                var request = new RestRequest(endpoint.ToString());

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content;
                    var obj = JsonConvert.DeserializeObject<Common.DTO.Response.AutoCompleteRoot>(data!);
                    var selectedData = new List<Prediction>();
                    foreach (var item in obj.Predictions)
                    {
                        await Task.Delay(255);
                        var placeDestination = await GetPlaceById(item.PlaceId);
                        selectedData.Add(new Prediction
                        {
                            Description = item.Description,
                            Compound = item.Compound,
                            Lat = placeDestination[0],
                            Lng = placeDestination[1]

                        });
                    }
                    result.Result = selectedData;
                }
            }
            catch (Exception e)
            {
                result = BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {e.Message} ");
            }
            return result;
        }

        public async Task<AppActionResult> Geocode(string address)
        {
            AppActionResult result = new();
            try
            {
                var endpoint = $"https://rsapi.goong.io/Geocode?api_key={APIKEY}&address={address}";
                var client = new RestClient();
                var request = new RestRequest(endpoint);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content;
                    var obj = JsonConvert.DeserializeObject<MapInfo.Root>(data!);
                    result.Result = obj;
                }
            }
            catch (Exception e)
            {
                result = BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {e.Message} ");
            }

            return result;
        }

        public async Task<AppActionResult> GetEstimateDeliveryResponse(double[] endCoordinate, double[]? startCoordinate)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var client = new RestClient();
                double[] start = startCoordinate;
                var endpoint = $"https://rsapi.goong.io/DistanceMatrix?origins={start[0]},{start[1]}&destinations={endCoordinate[0]},{endCoordinate[1]}&vehicle=bike&api_key={APIKEY}";
                var findDestinationRequest = new RestRequest(endpoint);

                var destinationResponse = await client.ExecuteAsync(findDestinationRequest);
                if (destinationResponse.IsSuccessStatusCode)
                {
                    var apiData = destinationResponse.Content;
                    if (!BuildAppActionResultIsError(result))
                    {
                        var estimatedTimeList = JsonConvert.DeserializeObject<EstimatedDeliveryTimeDto.Root>(apiData);
                        var data = new EstimatedDeliveryTimeDto.Response();
                        data.Elements = estimatedTimeList.Rows[0].Elements;
                        data.TotalDistance = (double)estimatedTimeList.Rows[0].Elements.Sum(e => e.Distance.Value) / 1000;
                        data.TotalDuration = (double)estimatedTimeList.Rows[0].Elements.Sum(e => e.Duration.Value) / 3600;
                        result.Result = data;
                    }
                }
            }
            catch (Exception e)
            {
                result.Result = BuildAppActionResultError(result, e.Message);

            }
            return result;
        }

        public async Task<double[]> GetPlaceById(string id)
        {
            double[] result = new double[2];
            try
            {
                var client = new RestClient();
                var endpoint = $"https://rsapi.goong.io/Place/Detail?place_id={id}&api_key={APIKEY}";
                var findDestinationRequest = new RestRequest(endpoint);

                var destinationResponse = await client.ExecuteAsync(findDestinationRequest);
                if (destinationResponse.IsSuccessStatusCode)
                {
                    var apiData = destinationResponse.Content;
                    if (apiData != null)
                    {
                        var place = JsonConvert.DeserializeObject<PlaceDetailResponse>(apiData);
                        if (place.status.Equals("OK")) 
                        {
                            result[0] = place.result.geometry.location.lat;
                            result[1] = place.result.geometry.location.lng;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                
            }
            return result;
        }
    }
}
