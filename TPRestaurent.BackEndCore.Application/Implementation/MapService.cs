using Newtonsoft.Json;
using RestSharp;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class MapService : GenericBackendService, IMapService
    {
        public const string APIKEY = "0Z6CMURRFI3YZufT2pPGjgWVdEqNo5tO73Cn9Q1y";
        private IGenericRepository<Configuration> _configurationRepository;
        private IUnitOfWork _unitOfWork;

        public MapService(IGenericRepository<Configuration> configurationRepository, IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configurationRepository = configurationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AutoComplete(MapAutoCompleteRequestDto dto)
        {
            AppActionResult result = new();

            try
            {
                StringBuilder endpoint = new StringBuilder();
                endpoint.Append($"https://rsapi.goong.io/Place/AutoComplete?api_key={APIKEY}");
                if (dto.Destination != null)
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

        public async Task<AppActionResult> CheckDistanceForChatBot(string address, string? customerName)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = "bạn";
                }
                var destinationResult = await Geocode(address);
                if (destinationResult.IsSuccess && destinationResult.Result != null)
                {
                    var destinationCoordinates = (destinationResult.Result as Root).Results[0].Geometry.Location;
                    var restaurantLatConfig = await _configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                    var restaurantLngConfig = await _configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);
                    var restaurantMaxDistanceToOrderConfig = await _configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DISTANCE_ORDER);

                    var restaurantLat = Double.Parse(restaurantLatConfig.CurrentValue);
                    var restaurantLng = Double.Parse(restaurantLngConfig.CurrentValue);
                    var maxDistanceToOrder = double.Parse(restaurantMaxDistanceToOrderConfig!.CurrentValue);

                    double[] restaurantAddress = new double[]
                    {
                            restaurantLat, restaurantLng
                    };

                    double[] customerAddress = new double[]
                    {
                            destinationCoordinates.Lat, destinationCoordinates.Lng
                    };

                    var distanceResponse = await GetEstimateDeliveryResponse(restaurantAddress, customerAddress);
                    var element = distanceResponse.Result as EstimatedDeliveryTimeDto.Response;
                    var distance = element!.TotalDistance;
                    if (distance > maxDistanceToOrder)
                    {
                        result.Result = $"Địa chỉ của {customerName} cách nhà hàng hơn {maxDistanceToOrder}km nên nhà hàng không thể giao hàng tận nơi";
                    }
                    else
                    {
                        result.Result = $"Có chứ, địa chỉ của {customerName} nằm trong phạm vi giao của nhà hàng";
                    }
                }
            }
            catch (Exception e)
            {
                result.Result = e;
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
                string baseUrl = "https://maps.goong.io/?location=";
                if (orderId == null)
                {
                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var restaurantLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                    var restaurantLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);
                    if (restaurantLatConfig != null && restaurantLngConfig != null)
                    {
                        var restaurantLat = restaurantLatConfig.CurrentValue.ToString();
                        var restaurantLng = restaurantLngConfig.CurrentValue.ToString();
                        result.Result = $"{baseUrl}{restaurantLat},{restaurantLng}";
                    }
                }
                else
                {
                    var addressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                    var orderRepository = Resolve<IGenericRepository<Order>>();
                    var orderDb = await orderRepository.GetByExpression(o => o.OrderId == orderId, o => o.Account);
                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {orderId}");
                    }
                    var addressDb = await addressRepository.GetAllDataByExpression(a => a.CustomerInfoAddressName.Equals(orderDb.Account.Address), 0, 0, null, false, null);
                    if (addressDb.Items.Count() > 0)
                    {
                        result.Result = $"{baseUrl}{addressDb.Items[0].Lat},{addressDb.Items[0].Lng}";
                    }
                    else
                    {
                        return BuildAppActionResultError(result, "Không tìm thấy địa chỉ trong hệ thống");
                    }
                }
            }
            catch (Exception ex)
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
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var configuraitonRepository = Resolve<IGenericRepository<Configuration>>();

                var startLat = await configuraitonRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                var startLng = await configuraitonRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);

                var orderToDeliver = await orderRepository!.GetAllDataByExpression(o => orderIds.Contains(o.OrderId) && o.OrderTypeId == OrderType.Delivery, 0, 0, null, false,
                    p => p.Account!,
                    p => p.Shipper!,
                    p => p.Status!,
                    p => p.OrderType!
                    );
                if (orderToDeliver.Items.Count() != orderIds.Count())
                {
                    result = BuildAppActionResultError(result, $"Tồn tại ít nhất 1 id đơn hàng không tồn tại");
                    return result;
                }

                List<(double Lat, double Lng)> coordinates = new List<(double Lat, double Lng)>();
                foreach (var order in orderToDeliver.Items)
                {
                    var customerInfoAddressDb = customerInfoAddressRepository!.GetByExpression(p => p.CustomerInfoAddressId == order.AddressId).Result;
                    if (customerInfoAddressDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ với id {order.AddressId}");
                    }

                    coordinates.Add((customerInfoAddressDb.Lat, customerInfoAddressDb.Lng));
                }

                string start = $"{startLat.CurrentValue.ToString()},{startLng.CurrentValue.ToString()}";
                StringBuilder waypoints = new StringBuilder();
                foreach (var item in coordinates)
                {
                    waypoints.Append($"{item.Lat.ToString()},{item.Lng.ToString()};");
                }

                waypoints.Remove(waypoints.Length - 1, 1);
                string endpoint = $"https://rsapi.goong.io/trip?origin={start}&waypoints={waypoints.ToString()}&vehicle=bike&api_key={APIKEY}";

                var client = new RestClient();
                var request = new RestRequest(endpoint);
                PagedResult<CustomerInfoAddress> customerInfoAddressList = new PagedResult<CustomerInfoAddress>();

                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content;
                    var obj = JsonConvert.DeserializeObject<TripInfo.Root>(data!);
                    OptimalTripResponseDTO optimalTripResponseDTO = new OptimalTripResponseDTO();
                    optimalTripResponseDTO.TotalDistance = obj.Trips[0].Distance;
                    optimalTripResponseDTO.TotalDuration = obj.Trips[0].Duration;
                    string placeEndPoint = null;
                    TripInfo.Leg currentTrip = null;
                    TripInfo.Waypoint currentWaypoint = null;
                    obj.Waypoints = obj.Waypoints.OrderBy(o => o.WaypointIndex).ToList();

                    HashSet<Guid> includedOrderIds = new HashSet<Guid>();
                    HashSet<Guid> customerInfoAddressIds = new HashSet<Guid>();

                    int index = 1;

                    for (int i = 1; i < obj.Trips[0].Legs.Count; i++)
                    {
                        currentTrip = obj.Trips[0].Legs[i - 1];
                        currentWaypoint = obj.Waypoints[i];

                        customerInfoAddressList = await customerInfoAddressRepository.GetAllDataByExpression(p => Math.Abs((double)(currentWaypoint.Location[0] - p.Lat)) <= 0.0007
                                                                  && Math.Abs((double)(currentWaypoint.Location[1] - p.Lng)) <= 0.001, 0, 0, null, false, null)!;
                        var customerInfoAddress = customerInfoAddressList.Items.OrderBy(c => Math.Pow(c.Lat - currentWaypoint.Location[0], 2) + Math.Pow(c.Lng - currentWaypoint.Location[1], 2)).FirstOrDefault();
                        if (customerInfoAddress == null && customerInfoAddressIds.Contains(customerInfoAddress.CustomerInfoAddressId))
                        {
                            continue;
                        }

                        customerInfoAddressIds.Add(customerInfoAddress.CustomerInfoAddressId);

                        if (customerInfoAddress != null)
                        {
                            var delivery = orderToDeliver.Items.Where(p => (customerInfoAddress.CustomerInfoAddressName.Contains(p.Account!.Address!) || p.Account!.Address.Contains(customerInfoAddress.CustomerInfoAddressName))
                                                                        && !includedOrderIds.Contains(p.OrderId)).ToList();
                            if (delivery != null && delivery.Count() > 0)
                            {
                                optimalTripResponseDTO.OptimalTrip!.Add(new RouteNode
                                {
                                    Index = index++,
                                    Orders = delivery,
                                    AccountId = delivery.FirstOrDefault().Account.Id,
                                    DistanceFromPreviousDestination = $"{Math.Round(currentTrip.Distance / 100) / 10} km",
                                    Duration = $"{Math.Round(currentTrip.Duration / 60)} phút",
                                });

                                foreach (var item in delivery)
                                {
                                    includedOrderIds.Add(item.OrderId);
                                }
                            }
                        }
                    }
                    result.Result = optimalTripResponseDTO.OptimalTrip.OrderBy(o => o.Index);
                }
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