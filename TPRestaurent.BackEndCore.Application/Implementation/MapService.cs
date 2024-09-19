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
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class MapService : GenericBackendService, IMapService
    {
        public const string APIKEY = "aZAqCAyyPhqquItUWec6O0D0QUZ32DPE6ni9D6al";
        private IUnitOfWork _unitOfWork;

        public MapService(IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AutoComplete(string address)
        {
            AppActionResult result = new();

            try
            {
                var endpoint = $"https://rsapi.goong.io/Place/AutoComplete?api_key={APIKEY}&input={address}";
                var client = new RestClient();
                var request = new RestRequest(endpoint);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = response.Content;
                    var obj = JsonConvert.DeserializeObject<Common.DTO.Response.AutoCompleteRoot>(data!);
                    var selectedData = obj.Predictions.Select(p => new Prediction
                    {
                        Description = p.Description,
                        Compound = p.Compound
                    }).ToList();
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

        public async Task<AppActionResult> GetEstimateDeliveryTime(double[] endCoordinate, double[]? startCoordinate)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var client = new RestClient();
                double[] start = startCoordinate;
                var endpoint = $"https://rsapi.goong.io/DistanceMatrix?origins={start[0]},{start[1]}&destinations={endCoordinate[0]},{endCoordinate[1]}&vehicle=bike&api_key={APIKEY}";
                var findDestinationRequest = new RestRequest(endpoint);

                var destinationResponse = await client.ExecuteAsync(findDestinationRequest);
                if (!BuildAppActionResultIsError(result))
                {
                    result.Result = JsonConvert.SerializeObject(destinationResponse);
                }
            }
            catch (Exception e)
            {


            }
            return result;
        }
    }
}
