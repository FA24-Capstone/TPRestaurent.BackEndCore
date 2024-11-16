using Castle.DynamicProxy.Generators;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ChatBotService : GenericBackendService, IChatBotService
    {
        private IGenericRepository<Account> _accountRepository;
        private IGenericRepository<Dish> _dishRepository;
        private IDishManagementService _dishManagementService;
        private IDishService _dishService;
        private IMapService _mapService;
        private ITableService _tableService;
        private IOpenAIClient _client;
        public ChatBotService(IGenericRepository<Account> accountRepository,
                              IGenericRepository<Dish> dishRepository,
                              ITableService tableService,
                              IDishService dishService,
                              IMapService mapService,
                              IDishManagementService dishManagementService,
                              IOpenAIClient client,
                              IServiceProvider service) : base(service)
        {
            _accountRepository = accountRepository;
            _dishRepository = dishRepository;
            _dishService = dishService;
            _tableService = tableService;
            _mapService = mapService;
            _dishManagementService = dishManagementService;
            _client = client;
        }

        public async Task<AppActionResult> ResponseCustomer(string customerId, string messageText)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var response = await _client.CreateChatCompletion($"{SD.OpenAIPrompt.HOT_FIX_PROMPT}{messageText}");
                if (response.Contains("trigger"))
                {
                    if (response.ToLower().Contains("table") 
                        || response.ToLower().Contains("reservation") 
                        || response.ToLower().Contains("booking")
                        || response.ToLower().Contains("event")
                        || response.ToLower().Contains("create")
                        || response.ToLower().Contains("make"))
                    {
                        string pattern = @"startTime=(\S+), endTime=(\S+), numOfPeople=(\d+)";
                        System.Text.RegularExpressions.Match match = Regex.Match(response, pattern);

                        // Check if we have a match
                        if (match.Success)
                        {
                            DateTime startTime = DateTime.Parse(match.Groups[1].Value);
                            DateTime endTime = DateTime.Parse(match.Groups[2].Value);
                            int numOfPeople = int.Parse(match.Groups[3].Value);
                            var findTable = await _tableService.FindTable(new Common.DTO.Request.FindTableDto
                            {
                                StartTime = startTime,
                                EndTime = endTime,
                                NumOfPeople = numOfPeople,
                                IsPrivate = false
                            });
                            if (findTable.IsSuccess && findTable.Result != null)
                            {
                                response = "Nhà hàng có bàn trống cho bạn vào khung giờ đó. Bạn hay truy cập https://thienphurestaurant.vercel.app/booking để đặt bàn nhé";
                            }
                        }
                        else
                        {
                            response = "Nhà hàng không có bàn trống vào khung giờ đó. Bạn hãy thử khung giờ khác nhé";
                        }
                    }
                    else if (response.ToLower().Contains("tags") && !response.ToLower().Contains("price"))
                    {
                        string[] split = response.Split(':');
                        string[] tags = split[1].Split(", ");
                        var suggestedDish = await _dishManagementService.GetDishWithTag(tags.ToList(), 3);
                        if (suggestedDish.IsSuccess && suggestedDish.Result != null)
                        {
                            List<string> dishes = suggestedDish.Result as List<string>;
                            response = $"Nhà hàng gợi ý cho bạn dùng thử: {string.Join(", ", dishes)}";
                        }
                        else
                        {
                            response = "Bạn có thể khám phá các món của nhà hàng ở Thực đơn và combo";
                        }
                    }
                    else if (response.ToLower().Contains("distance") 
                        || response.ToLower().Contains("calculate")
                        || response.ToLower().Contains("delivery")
                        || response.ToLower().Contains("location")
                        || response.ToLower().Contains("giao")
                        || response.ToLower().Contains("hang")
                        || response.ToLower().Contains("api")
                        )
                    {
                        string[] extract = response.Split(new char[] { ':', '=' }, StringSplitOptions.RemoveEmptyEntries);
                        // Extract the destination value
                        string destination = extract[extract.Length - 1];
                        var distanceResult = await _mapService.CheckDistanceForChatBot(destination);
                        if (distanceResult.IsSuccess && distanceResult.Result != null)
                        {
                            response = distanceResult.Result as string;
                        }
                        else
                        {
                            response = "Nhà hàng chỉ hỗ trợ cho đơn giao hàng trong bán kính 10km";
                        }
                    }
                    else if (response.ToLower().Contains("name") || response.ToLower().Contains("price"))
                    {
                        string dishName = response.Split(", ")[0].Split(": ")[1];
                        var dishResult = await _dishService.GetAllDish(dishName, null, 1, 3, null, null);
                        if (dishResult.IsSuccess && dishResult.Result != null)
                        {
                            var dishDb = dishResult.Result as PagedResult<DishSizeResponse>;
                            if (dishDb != null && dishDb.Items.Count > 0)
                            {
                                response = $"Bạn có thể thử {string.Join(", ", dishDb.Items.Select(d => d.Dish.Name))} tại nhà hàng";
                            }
                            else
                            {
                                response = "Bạn có thể khám phá các món của nhà hàng ở Thực đơn và combo";
                            }
                        }
                    }
                }
                result.Result = response;
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
