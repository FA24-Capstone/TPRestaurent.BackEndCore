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
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;
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

        public async Task<AppActionResult> GetGoogleMapLink(Guid? orderId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                string baseUrl = "https://www.google.com/maps/place/@";
                if (orderId == null)
                {
                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var restaurantLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                    var restaurantLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);
                    if(restaurantLatConfig != null && restaurantLngConfig != null)
                    {
                        var restaurantLat = restaurantLatConfig.CurrentValue.ToString();
                        var restaurantLng = restaurantLngConfig.CurrentValue.ToString();
                        result.Result = $"{baseUrl}{restaurantLat},{restaurantLng}";
                    }
                } else
                {
                    var addressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                    var orderRepository = Resolve<IGenericRepository<Order>>();
                    var orderDb = await orderRepository.GetById(orderId);
                    if(orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn hàn với id {orderId}");
                    }
                    //var addressDb = await addressRepository.GetByExpression(a => a.CustomerInfoAddressName.Equals(orderDb.))
                }
            } catch(Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetOptimalPath(List<Guid> orderIds)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var orderToDeliver = await orderRepository!.GetAllDataByExpression(o => orderIds.Contains(o.OrderId), 0, 0, null, false, null);
                if (orderToDeliver.Items.Count() != orderIds.Count())
                {
                    result = BuildAppActionResultError(result, $"Tồn tại ít nhất 1 id đơn hàng không tồn tại");
                    return result;
                }

                if (orderToDeliver.Items.Where(o => o.OrderTypeId != Domain.Enums.OrderType.Delivery || o.StatusId != Domain.Enums.OrderStatus.ReadyForDelivery).Count() > 0)
                {
                    result = BuildAppActionResultError(result, $"Tồn tại ít nhất 1 id đơn hàng không tồn tại");
                    return result;
                }

                //var onWayProvinces = await provinceRepository!.GetAllDataByExpression(p => DestinationIds.Contains(p.Id), 0, 0, null, false, null);
                //if (onWayProvinces.Items!.Count != DestinationIds.Count)
                //{
                //    foreach (var item in onWayProvinces.Items)
                //    {
                //        if (!DestinationIds.Contains(item.Id))
                //        {
                //            result = BuildAppActionResultError(result, $"Không tìm thấy thông tin địa điểm với id {item.Id}");
                //            return result;
                //        }
                //    }
                //}

                //if (true)
                //{
                //    string start = $"{startProvince.lat.ToString()},{startProvince.lng.ToString()}";
                //    StringBuilder waypoints = new StringBuilder();
                //    foreach (var item in onWayProvinces.Items)
                //    {
                //        waypoints.Append($"{item.lat.ToString()},{item.lng.ToString()};");
                //    }
                //    waypoints.Remove(waypoints.Length - 1, 1);
                //    string endpoint = $"https://rsapi.goong.io/trip?origin={start}&waypoints={waypoints.ToString()}&api_key={APIKEY}";
                //    var client = new RestClient();
                //    var request = new RestRequest(endpoint);

                //    var response = await client.ExecuteAsync(request);

                //    if (response.IsSuccessStatusCode)
                //    {
                //        var data = response.Content;
                //        var obj = JsonConvert.DeserializeObject<TripInfo.Root>(data!);
                //        OptimalTripResponseDTO optimalTripResponseDTO = new OptimalTripResponseDTO();
                //        optimalTripResponseDTO.TotalDistance = obj.Trips[0].Distance;
                //        optimalTripResponseDTO.TotalDuration = obj.Trips[0].Duration;
                //        string placeEndPoint = null;
                //        TripInfo.Leg currentTrip = null;
                //        TripInfo.Waypoint currentWaypoint = null;
                //        //List<RestResponse> provinceResponse = new List<RestResponse>();
                //        obj.Waypoints = obj.Waypoints.OrderBy(o => o.WaypointIndex).ToList();

                //        //for(int i = 0; i < obj.Trips[0].Legs.Count; i++)
                //        //{
                //        //    placeEndPoint = $"https://rsapi.goong.io/Place/Detail?place_id={obj.Waypoints[i].PlaceId}&api_key={APIKEY}";
                //        //    response = await client.ExecuteAsync(new RestRequest(placeEndPoint));
                //        //    provinceResponse.Add(response);
                //        //    await Task.Delay(350);
                //        //}
                //        Province currentProvinceName = null;

                //        for (int i = 0; i < obj.Trips[0].Legs.Count; i++)
                //        {
                //            currentTrip = obj.Trips[0].Legs[i];
                //            currentWaypoint = obj.Waypoints[i];
                //            currentProvinceName = (await provinceRepository.GetByExpression(p => Math.Abs((double)(currentWaypoint.Location[0] - p.lat)) <= 0.0007
                //                                                                              && Math.Abs((double)(currentWaypoint.Location[1] - p.lng)) <= 0.001))!;
                //            if (currentProvinceName != null)
                //            {
                //                if (currentProvinceName.Name.Contains(startProvince.Name) || startProvince.Name.Contains(currentProvinceName.Name))
                //                {
                //                    optimalTripResponseDTO.OptimalTrip!.Add(new RouteNode
                //                    {
                //                        Index = currentWaypoint.WaypointIndex,
                //                        ProvinceId = startProvince.Id,
                //                        ProvinceName = startProvince.Name,
                //                        VehicleToNextDestination = VehicleType.BUS,
                //                        DistanceToNextDestination = currentTrip.Distance,
                //                        Duration = currentTrip.Duration,
                //                    });
                //                    continue;
                //                }
                //                var province = onWayProvinces.Items.FirstOrDefault(p => currentProvinceName.Name.Contains(p.Name) || p.Name.Contains(currentProvinceName.Name));
                //                if (province != null)
                //                {
                //                    optimalTripResponseDTO.OptimalTrip!.Add(new RouteNode
                //                    {
                //                        Index = currentWaypoint.WaypointIndex,
                //                        ProvinceId = province.Id,
                //                        ProvinceName = province.Name,
                //                        VehicleToNextDestination = VehicleType.BUS,
                //                        DistanceToNextDestination = currentTrip.Distance,
                //                        Duration = currentTrip.Duration,
                //                    });
                //                }
                //            }
                //        }

                //        result.Result = optimalTripResponseDTO.OptimalTrip.OrderBy(o => o.Index);
                //    }
                //}



            }
            catch (Exception e)
            {
                result = BuildAppActionResultError(result, e.Message);
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
