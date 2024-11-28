using System.Text;
using System.Text.RegularExpressions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ChatBotService : GenericBackendService, IChatBotService
    {
        private IGenericRepository<Account> _accountRepository;
        private IGenericRepository<Dish> _dishRepository;
        private IGenericRepository<Combo> _comboRepository;
        private IGenericRepository<Rating> _ratingRepository;
        private IGenericRepository<CustomerInfoAddress> _addressRepository;
        private IGenericRepository<Domain.Models.Configuration> _configurationRepository;
        private IDishManagementService _dishManagementService;
        private IDishService _dishService;
        private IMapService _mapService;
        private ITableService _tableService;
        private IOpenAIClient _client;

        public ChatBotService(IGenericRepository<Account> accountRepository,
                              IGenericRepository<Dish> dishRepository,
                              IGenericRepository<Combo> comboRepository,
                              IGenericRepository<Rating> ratingRepository,
                              IGenericRepository<CustomerInfoAddress> addressRepository,
        IGenericRepository<Domain.Models.Configuration> configurationRepository,
                              ITableService tableService,
                              IDishService dishService,
                              IMapService mapService,
                              IDishManagementService dishManagementService,
                              IOpenAIClient client,
                              IServiceProvider service) : base(service)
        {
            _accountRepository = accountRepository;
            _dishRepository = dishRepository;
            _comboRepository = comboRepository;
            _ratingRepository = ratingRepository;
            _addressRepository = addressRepository;
            _configurationRepository = configurationRepository;
            _dishService = dishService;
            _tableService = tableService;
            _mapService = mapService;
            _dishManagementService = dishManagementService;
            _client = client;
        }

        public async Task<AppActionResult> ResponseCustomer(ChatbotRequestDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                StringBuilder request = new StringBuilder();
                string customerGreeting = "bạn";
                if (!string.IsNullOrEmpty(dto.CustomerId))
                {
                    var accountDb = await _accountRepository.GetById(dto.CustomerId);
                    if (accountDb != null)
                    {
                        var dishDb = await _ratingRepository.GetAllDataByExpression(r => r.OrderDetail.Order.AccountId.Equals(dto.CustomerId), 1, 6, o => o.PointId, false, o => o.OrderDetail.DishSizeDetail.Dish, o => o.OrderDetail.Combo);
                        var dishName = string.Join(", ", dishDb.Items.OrderByDescending(d => d.CreateDate).Take(2).Select(c =>
                        {
                            if (c.OrderDetail.DishSizeDetailId.HasValue)
                            {
                                return c.OrderDetail.DishSizeDetail.Dish.Name;
                            }
                            else
                            {
                                return c.OrderDetail.Combo.Name;
                            }
                        }));

                        customerGreeting = SD.OpenAIPrompt.GetCustomerGreeting(accountDb, dishName, dto.IsFirstCall);
                        if (dto.IsFirstCall)
                        {
                            result.Result = customerGreeting;
                            return result;
                        }
                        else
                        {
                            request.Append($"Gọi khách là {customerGreeting}");
                        }

                        var customerInfoAddress = await _addressRepository.GetByExpression(a => a.AccountId.Equals(dto.CustomerId) && !a.IsDeleted && a.IsCurrentUsed);
                        if (customerInfoAddress != null)
                        {
                            request.Append($"Nếu khách hỏi có giao hàng không mà không nêu địa chỉ cụ thể hay chứa giao tới nhà tôi-> địa chỉ là {customerInfoAddress.CustomerInfoAddressName}.");
                        }
                    }
                }

                request.Append($"{SD.OpenAIPrompt.HOT_FIX_PROMPT}{dto.Message}");
                var response = await _client.CreateChatCompletion(request.ToString());
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
                        System.Text.RegularExpressions.Match match = Regex.Match(ParseDateTimeString(response), pattern);

                        // Check if we have a match
                        if (match.Success)
                        {
                            var openHour = await _configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.OPEN_TIME));
                            var closedHour = await _configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.CLOSED_TIME));
                            DateTime startTime = DateTime.Parse(match.Groups[1].Value);
                            DateTime endTime = DateTime.Parse(match.Groups[2].Value);
                            if (double.Parse(openHour.CurrentValue) > startTime.Hour
                                || double.Parse(closedHour.CurrentValue) < endTime.Hour)
                            {
                                result.Result = $"Nhà hàng chỉ hoạt động từ {openHour.CurrentValue}h đến {closedHour.CurrentValue}h";
                                return result;
                            }

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
                                response = $"Nhà hàng có bàn trống cho {customerGreeting} vào khung giờ đó. {customerGreeting} hay truy cập https://thienphurestaurant.vercel.app/booking để đặt bàn nhé";
                            }
                        }
                        else
                        {
                            response = $"Nhà hàng không có bàn trống vào khung giờ đó. {customerGreeting} hãy thử khung giờ khác nhé";
                        }
                    }
                    else if (response.ToLower().Contains("tags"))
                    {
                        var searchDish = ParseDishInfo(response);
                        var suggestedDish = await _dishManagementService.GetDishWithTag(searchDish.Tags, 3, searchDish.PriceRange.Min, searchDish.PriceRange.Max);
                        if (suggestedDish.IsSuccess && suggestedDish.Result != null)
                        {
                            List<string> dishes = suggestedDish.Result as List<string>;
                            response = $"Nhà hàng gợi ý cho {customerGreeting} dùng thử: {string.Join(", ", dishes)}";
                        }
                        else
                        {
                            response = $"{customerGreeting} có thể khám phá các món của nhà hàng ở Thực đơn và combo";
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
                        var distanceResult = await _mapService.CheckDistanceForChatBot(destination, customerGreeting);
                        if (distanceResult.IsSuccess && distanceResult.Result != null)
                        {
                            response = distanceResult.Result as string;
                        }
                        else
                        {
                            response = "Nhà hàng chỉ hỗ trợ cho đơn giao hàng trong bán kính 10km";
                        }
                    }
                    else if (response.ToLower().Contains("name"))
                    {
                        var searchDish = ParseDishInfo(response);
                        var dishList = await _dishRepository.GetAllDataByExpression(p => p.Name.ToLower().Contains(searchDish.Name.ToLower())
                                             && p.IsMainItem && !p.IsDeleted && p.isAvailable, 1, 3, null, false, null);
                        if (dishList.Items.Count > 0)
                        {
                            response = $"{customerGreeting} có thể thử {string.Join(", ", dishList.Items.Select(d => d.Name))} tại nhà hàng";
                        }
                        else
                        {
                            var comboList = await _comboRepository.GetAllDataByExpression(p => p.Name.ToLower().Contains(searchDish.Name.ToLower())
                                            && !p.IsDeleted && p.IsAvailable, 1, 3, null, false, null);
                            if (dishList.Items.Count > 0)
                            {
                                response = $"{customerGreeting} có thể thử {string.Join(", ", comboList.Items.Select(d => d.Name))} tại nhà hàng";
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

        private DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysUntilNext = ((int)day - (int)start.DayOfWeek + 7) % 7;
            if (daysUntilNext == 0) daysUntilNext = 7;  // Move to next week if it's the same day
            return start.AddDays(daysUntilNext);
        }

        // Main method to parse and replace placeholders in the string
        private string ParseDateTimeString(string input)
        {
            var utility = Resolve<Utility>();
            DateTime today = utility.GetCurrentDateTimeInTimeZone();

            // Dictionary to store pre-calculated dates for each placeholder
            var replacements = new System.Collections.Generic.Dictionary<string, DateTime>
        {
            { "{{TODAY}}", today },
            { "{{TODAY+1}}", today.AddDays(1) },
            { "{{TODAY+2}}", today.AddDays(2) },
            { "{{NEXT_MONDAY}}", GetNextWeekday(today, DayOfWeek.Monday) },
            { "{{NEXT_TUESDAY}}", GetNextWeekday(today, DayOfWeek.Tuesday) },
            { "{{NEXT_WEDNESDAY}}", GetNextWeekday(today, DayOfWeek.Wednesday) },
            { "{{NEXT_THURSDAY}}", GetNextWeekday(today, DayOfWeek.Thursday) },
            { "{{NEXT_FRIDAY}}", GetNextWeekday(today, DayOfWeek.Friday) },
            { "{{NEXT_SATURDAY}}", GetNextWeekday(today, DayOfWeek.Saturday) },
            { "{{NEXT_SUNDAY}}", GetNextWeekday(today, DayOfWeek.Sunday) },
            { "{{NEXT_MONDAY+7}}", GetNextWeekday(today, DayOfWeek.Monday).AddDays(7) },
            { "{{NEXT_TUESDAY+7}}", GetNextWeekday(today, DayOfWeek.Tuesday).AddDays(7) },
            { "{{NEXT_WEDNESDAY+7}}", GetNextWeekday(today, DayOfWeek.Wednesday).AddDays(7) },
            { "{{NEXT_THURSDAY+7}}", GetNextWeekday(today, DayOfWeek.Thursday).AddDays(7) },
            { "{{NEXT_FRIDAY+7}}", GetNextWeekday(today, DayOfWeek.Friday).AddDays(7) },
            { "{{NEXT_SATURDAY+7}}", GetNextWeekday(today, DayOfWeek.Saturday).AddDays(7) },
            { "{{NEXT_SUNDAY+7}}", GetNextWeekday(today, DayOfWeek.Sunday).AddDays(7) },
            { "{{NEXT_TWO_MONDAY+14}}", GetNextWeekday(today, DayOfWeek.Monday).AddDays(14) },
        };

            // Regex pattern to match placeholders
            string pattern = @"\{\{(TODAY(?:\+\d+)?|NEXT_(?:MONDAY|TUESDAY|WEDNESDAY|THURSDAY|FRIDAY|SATURDAY|SUNDAY)(?:\+\d+)?)\}\}";
            var regex = new Regex(pattern);

            // Replace each placeholder in the input string with its actual date
            var result = regex.Replace(input, match =>
            {
                string placeholder = match.Value;
                if (replacements.TryGetValue(placeholder, out DateTime dateValue))
                {
                    return dateValue.ToString("yyyy-MM-dd");
                }
                return placeholder; // Leave unchanged if not found
            });

            return result;
        }

        public SearchDishInfo ParseDishInfo(string input)
        {
            // Regular expressions to capture dish name, tags, and price
            var nameRegex = new Regex(@"trigger_find_dish_name:\s*([\w\s]+)");
            var tagsRegex = new Regex(@"trigger_dish_tags:\s*([\w\s,]+)");
            var priceRegex = new Regex(@"(?:price|trigger_price)=(<|>|<=|>=)?([\d+-]+)");

            // Match dish name
            var nameMatch = nameRegex.Match(input);
            string dishName = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : string.Empty;

            // Match tags
            var tagsMatch = tagsRegex.Match(input);
            var tags = new List<string>();
            if (tagsMatch.Success)
            {
                tags = new List<string>(tagsMatch.Groups[1].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                tags = tags.ConvertAll(tag => tag.Trim());
            }

            // Match price with comparison operator
            (decimal? Min, decimal? Max) priceRange = (null, null);
            var priceMatch = priceRegex.Match(input);
            if (priceMatch.Success)
            {
                var comparison = priceMatch.Groups[1].Value; // Comparison operator: <, >, <=, >=
                var priceString = priceMatch.Groups[2].Value;

                // Parse price based on comparison operator
                if (comparison == "<")
                {
                    priceRange = (null, decimal.Parse(priceString)); // Max set, Min is null
                }
                else if (comparison == ">")
                {
                    priceRange = (decimal.Parse(priceString), null); // Min set, Max is null
                }
                else if (priceString.Contains('-')) // Range case (min-max)
                {
                    var prices = priceString.Split('-');
                    priceRange = (decimal.Parse(prices[0]), decimal.Parse(prices[1]));
                }
                else // Fixed Price or >=, <= case
                {
                    var priceValue = decimal.Parse(priceString);
                    if (comparison == "<=")
                        priceRange = (null, priceValue);
                    else if (comparison == ">=")
                        priceRange = (priceValue, null);
                    else
                        priceRange = (priceValue, priceValue); // Fixed price
                }
            }

            return new SearchDishInfo { Name = dishName, Tags = tags, PriceRange = priceRange };
        }
    }
}