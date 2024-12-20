using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TableService : GenericBackendService, ITableService
    {
        private readonly IGenericRepository<Table> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public TableService(IGenericRepository<Table> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppActionResult> CreateTable(TableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                //Name validation (if needed)
                if (string.IsNullOrEmpty(dto.TableName))
                {
                    return BuildAppActionResultError(result, "Tên bàn không được để trống");
                    return result;
                }

                if ((int)dto.TableSizeId < 1)
                {
                    return BuildAppActionResultError(result, "Số ghế ngồi phải lớn hơn 0");
                    return result;
                }

                var tableRatingRepository = Resolve<IGenericRepository<Room>>();
                if ((await tableRatingRepository.GetById(dto.TableRatingId) == null))
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phòng với id {dto.TableRatingId}");
                    return result;
                }

                var newTable = _mapper.Map<Table>(dto);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.DevicePassword);
                newTable.TableId = Guid.NewGuid();
                newTable.IsDeleted = false;
                newTable.DeviceCode = dto.DeviceCode;
                newTable.DevicePassword = hashedPassword;
                newTable.TableStatusId = TableStatus.NEW;
                newTable.Coordinates = "(0,0)";
                await _repository.Insert(newTable);

                var tableSetUpConfig = await congfigurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TABLE_IS_SET_UP), null);
                if (tableSetUpConfig != null)
                {
                    tableSetUpConfig.CurrentValue = SD.DefaultValue.NEW;
                    await congfigurationRepository.Update(tableSetUpConfig);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> FindTable(FindTableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderService = Resolve<IOrderService>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                // Fetch configuration values
                var openTime = double.Parse((await configurationRepository
                    .GetByExpression(c => c.Name.Equals(SD.DefaultValue.OPEN_TIME), null))?.CurrentValue ?? "0");
                var closedTime = double.Parse((await configurationRepository
                    .GetByExpression(c => c.Name.Equals(SD.DefaultValue.CLOSED_TIME), null))?.CurrentValue ?? "0");
                var minPeople = int.Parse((await configurationRepository
                    .GetByExpression(c => c.Name.Equals(SD.DefaultValue.MIN_PEOPLE_FOR_RESERVATION), null))?.CurrentValue ?? "0");
                var maxPeople = int.Parse((await configurationRepository
                    .GetByExpression(c => c.Name.Equals(SD.DefaultValue.MAX_PEOPLE_FOR_RESERVATION), null))?.CurrentValue ?? "0");

                // Ensure EndTime is set
                if (!dto.EndTime.HasValue)
                {
                    dto.EndTime = dto.StartTime.AddHours(1); // Default to 1 hour duration
                }

                // Validate booking time
                bool isInvalidTime =
                    dto.StartTime.Date.AddHours(openTime) > dto.StartTime ||
                    dto.StartTime.Date.AddHours(closedTime) < dto.EndTime.Value;

                if (isInvalidTime)
                {
                    throw new Exception("Thời gian đặt không hợp lệ");
                }

                // Validate number of people
                bool isInvalidPeopleCount = dto.NumOfPeople < minPeople || dto.NumOfPeople > maxPeople;

                if (isInvalidPeopleCount)
                {
                    throw new Exception("Số người không hợp lệ");
                }


                var availableTables = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.IsPrivate, dto.NumOfPeople, 0, 0);
                if (availableTables.Count == 0)
                {
                    result.Messages.Add("Không tìm thấy bàn cho yêu cầu đặt bàn");
                    return result;
                }
                // Handle private table finding -> 1 table allowed
                if (dto.IsPrivate)
                {
                    if(dto.NumOfPeople > 11)
                    {
                        result.Messages.Add("Không tìm thấy bàn cho yêu cầu đặt bàn. Nhà hàng gợi ý quý khách có thể thử với bàn ở không gian chung.");
                        return result;
                    }

                    var data = _mapper.Map<TableArrangementResponseItem>(availableTables.FirstOrDefault());
                    result.Result = new List<TableArrangementResponseItem> { data };
                    if(dto.NumOfPeople < 4)
                    {
                        result.Messages.Add("Số người đang khá ít so với kích thước phòng riêng tư nhà hiện có. Nhà hàng gợi ý quý khách có thể thử với bàn ở không gian chung.");
                    }
                    return result;                    
                }
                //handle public table finding -> Mulpltiple tables allowed
                var bestCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, false);
                if (bestCaseFindPublicTableResult.IsSuccess && bestCaseFindPublicTableResult.Result != null)
                {
                    result.Result = bestCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                    return result;
                }

                var badCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, true);
                if (badCaseFindPublicTableResult.IsSuccess && badCaseFindPublicTableResult.Result != null)
                {
                    result.Result = badCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                    if (badCaseFindPublicTableResult.Messages.Count > 0)
                    {
                        result.Messages.AddRange(badCaseFindPublicTableResult.Messages);
                    }
                    return result;
                }

                if (dto.NumOfPeople < 4)
                {
                    dto.NumOfPeople = 4;
                    var exceedingCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, false);
                    if (exceedingCaseFindPublicTableResult.IsSuccess && exceedingCaseFindPublicTableResult.Result != null)
                    {
                        result.Result = exceedingCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                        return result;
                    }
                }

                if (dto.NumOfPeople < 6)
                {
                    dto.NumOfPeople = 6;
                    var exceedingCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, false);
                    if (exceedingCaseFindPublicTableResult.IsSuccess && exceedingCaseFindPublicTableResult.Result != null)
                    {
                        result.Result = exceedingCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                        return result;
                    }
                }

                if (dto.NumOfPeople < 8)
                {
                    dto.NumOfPeople = 8;
                    var exceedingCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, false);
                    if (exceedingCaseFindPublicTableResult.IsSuccess && exceedingCaseFindPublicTableResult.Result != null)
                    {
                        result.Result = exceedingCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                        return result;
                    }
                }

                if (dto.NumOfPeople < 10)
                {
                    dto.NumOfPeople = 10;
                    var exceedingCaseFindPublicTableResult = await FindPublicTableWithCase(dto, availableTables, false);
                    if (exceedingCaseFindPublicTableResult.IsSuccess && exceedingCaseFindPublicTableResult.Result != null)
                    {
                        result.Result = exceedingCaseFindPublicTableResult.Result as List<TableArrangementResponseItem>;
                        return result;
                    }
                }
                return BuildAppActionResultError(result, $"Nhà hàng không xếp bàn cho quý khách. Xin lỗi và xin hẹn quý khách lần sau");
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<AppActionResult> FindPublicTableWithCase(FindTableDto dto, List<Table> availableTables, bool allowAddChair)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var data = new List<TableArrangementResponseItem>();
                int[] sizes = null;
                if (allowAddChair)
                {
                    sizes = new int[] { 2, 4, 6, 9, 11 }; // Possible sizes
                }
                else
                {
                    sizes = new int[] { 2, 4, 6, 8, 10 }; // Possible sizes
                }
                int target = dto.NumOfPeople + 2;
                List<List<int>> possibleTableSet = new List<List<int>>();
                await Backtrack(possibleTableSet, new List<int>(), sizes, dto.NumOfPeople, 0, target);

                // Filter out subsets that are not optimal
                possibleTableSet = await FilterOptimalSubsets(possibleTableSet, dto.NumOfPeople);
                var tableSizeDictionary = availableTables.GroupBy(a => (int)a.TableSizeId).ToDictionary(a => a.Key, a => a.Count());
                var adjustedTableSizeDictionary = new Dictionary<int, int>();
                if (allowAddChair)
                {
                    foreach (var table in tableSizeDictionary)
                    {
                        if (table.Key == 8 || table.Key == 10)
                        {
                            adjustedTableSizeDictionary.Add(table.Key + 1, table.Value);
                        }
                        else
                        {
                            adjustedTableSizeDictionary.Add(table.Key, table.Value);
                        }
                    }
                }
                else
                {
                    adjustedTableSizeDictionary = tableSizeDictionary;
                }
                possibleTableSet = await FilterAvailableQuantity(possibleTableSet, adjustedTableSizeDictionary);
                foreach (var possibleTable in possibleTableSet)
                {
                    for (int i = 0; i < possibleTable.Count; i++)
                    {
                        if (possibleTable[i] == 9)
                        {
                            possibleTable[i] = 8;
                        }
                        else if (possibleTable[i] == 11)
                        {
                            possibleTable[i] = 10;
                        }
                    }
                    var tableList = await FindBestTables(new List<Table>(availableTables), possibleTable);
                    if (tableList != null && tableList.Count > 0)
                    {
                        int tableSizeCheck = 0;
                        foreach (var table in tableList)
                        {
                            var tableResponse = _mapper.Map<TableArrangementResponseItem>(table);
                            List<(int, int)> tableCoordinates = DeserializeList(table.Coordinates);
                            if (tableCoordinates.Count > 0)
                            {
                                tableResponse.Position.X = tableCoordinates.FirstOrDefault().Item1;
                                tableResponse.Position.Y = tableCoordinates.FirstOrDefault().Item2;
                            }
                            data.Add(tableResponse);
                            tableSizeCheck += (int)table.TableSizeId;
                        }
                        result.Result = data;
                        if (tableSizeCheck < dto.NumOfPeople)
                        {
                            result.Messages.Add("Lịch đặt bàn hiện tại đang khá dày nên nhà hàng sẽ phải kê thêm ghế cho quý khách");
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<List<List<int>>> FilterAvailableQuantity(List<List<int>> possibleTableSet, Dictionary<int, int> dictionary)
        {
            List<List<int>> reducedList = new List<List<int>>();
            try
            {
                bool isValid = true;
                foreach (var possibleTable in possibleTableSet)
                {
                    isValid = true;
                    var dictionaryKey = dictionary.Select(x => x.Key).OrderBy(x => x).ToList();
                    var possibleTableKey = possibleTable.Distinct().OrderBy(x => x).ToList();
                    if (dictionaryKey.All(possibleTableKey.Contains))
                    {
                        continue;
                    }
                    foreach (var kvp in dictionary)
                    {
                        if (possibleTable.Count(p => p == kvp.Key) > kvp.Value)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        reducedList.Add(possibleTable);
                    }
                }
            }
            catch (Exception ex)
            {
                return possibleTableSet;
            }
            return reducedList;
        }

        private async Task Backtrack(List<List<int>> result, List<int> current, int[] sizes, int number, int start, int target)
        {
            int sum = current.Sum();

            // If the sum is within the acceptable range, add to the result
            if (sum >= number && sum <= target)
            {
                result.Add(new List<int>(current));
                return;
            }

            // Try to add each size to the current subset
            for (int i = start; i < sizes.Length; i++)
            {
                if (sum + sizes[i] > target)
                {
                    break;
                }
                current.Add(sizes[i]);
                Backtrack(result, current, sizes, number, i, target);
                current.RemoveAt(current.Count - 1); // Backtrack step
            }
        }

        private async Task<List<List<int>>> FilterOptimalSubsets(List<List<int>> subsets, int NumOfPeople)
        {
            List<List<int>> optimalSubsets = new List<List<int>>();

            foreach (var subset in subsets)
            {
                bool isOptimal = true;

                // Check if any element can be removed while still satisfying the condition
                foreach (var element in subset)
                {
                    var reducedSubset = new List<int>(subset);
                    reducedSubset.Remove(element);

                    int reducedSum = reducedSubset.Sum();
                    if (reducedSum > subset.Sum() - element)
                    {
                        isOptimal = false;
                        break;
                    }
                }

                if (isOptimal)
                {
                    optimalSubsets.Add(subset);
                }
            }

            return optimalSubsets.OrderBy(o => Math.Abs(o.Sum(i => i) - NumOfPeople)).ThenBy(o => o.Count()).ThenBy(o =>
            {
                double mean = o.Average();
                return o.Average(or => Math.Pow(or - mean, 2));
            }).ToList();
        }

        //private async Task<List<Table>> FindBestTables(List<Table> tables, List<int> requestedSizes)
        //{
        //    //List<Table> bestCombination = null;
        //    //Backtrack(new List<Table>(tables.OrderBy(t => t.Coordinates)), requestedSizes.OrderBy(r => r).ToList(), new List<Table>(), ref bestCombination, 0);
        //    //return bestCombination;

        //    List<List<Table>> allValidCombinations = new List<List<Table>>();
        //    List<Table> currentCombination = new List<Table>();
        //    int highestProximity = -1;
        //    List<Table> bestCombination = null;

        //    // Sort tables by TableSizeId to optimize the combination generation
        //    var sortedTables = tables.OrderBy(t => t.TableSizeId).ToList();

        //    // Start backtracking to find all valid combinations
        //    BacktrackFindCombinations(sortedTables, requestedSizes, 0, currentCombination, allValidCombinations);

        //    // Iterate through all valid combinations to find the one with the highest proximity
        //    foreach (var combination in allValidCombinations)
        //    {
        //        int totalProximity = CalculateTotalProximity(combination);
        //        if (totalProximity > highestProximity)
        //        {
        //            highestProximity = totalProximity;
        //            bestCombination = combination;
        //        }
        //    }

        //    return bestCombination;
        //}

        //    private void Backtrack(List<Table> tables, List<int> sizes, List<Table> currentTables, ref List<Table> bestCombination, int startIndex)
        //    {
        //        // If all requested sizes are used, check if this combination is valid
        //        if (sizes.Count == 0)
        //        {
        //            if (bestCombination == null || currentTables.Count < bestCombination.Count)
        //            {
        //                bestCombination = new List<Table>(currentTables);
        //            }
        //            return;
        //        }

        //        // Try to find the best table of the current requested size
        //        int maxProximity = -1;
        //        Table bestTable = null;

        //        for (int i = startIndex; i < tables.Count; i++)
        //        {
        //            int tableSize = (int)tables[i].TableSizeId; // Each cell represents 2 units

        //            if (tableSize == sizes[0])
        //            {
        //                int proximity = CalculateProximity(currentTables, tables[i]);
        //                // If this table is closer to the existing tables, update the best choice
        //                if (proximity > maxProximity)
        //                {
        //                    maxProximity = proximity;
        //                    bestTable = tables[i];
        //                }
        //            }
        //        }

        //        // If a best table is found, proceed with backtracking
        //        if (bestTable != null)
        //        {
        //            currentTables.Add(bestTable);
        //            Backtrack(tables.Where(t => t.TableId != bestTable.TableId).ToList(), sizes.Skip(1).ToList(), currentTables, ref bestCombination, startIndex);
        //            currentTables.RemoveAt(currentTables.Count - 1); // Backtrack step
        //        }
        //    }

        //    private int CalculateProximity(List<Table> currentTables, Table newTable)
        //    {
        //        int proximity = 0;

        //        foreach (var currentTable in currentTables)
        //        {
        //            foreach (var cell in DeserializeList(currentTable.Coordinates))
        //            {
        //                foreach (var newCell in DeserializeList(newTable.Coordinates))
        //                {
        //                    // Check if cells are adjacent (horizontally or vertically)
        //                    if (Math.Abs(cell.Item1 - newCell.Item1) + Math.Abs(cell.Item2 - newCell.Item2) == 1)
        //                    {
        //                        proximity++;
        //                    } 
        //                }
        //            }
        //        }
        //        return proximity;
        //    }

        //    private void BacktrackFindCombinations(
        //List<Table> tables,
        //List<int> sizes,
        //int startIndex,
        //List<Table> currentCombination,
        //List<List<Table>> allValidCombinations)
        //    {
        //        // If all sizes have been accommodated, add the current combination to the list
        //        if (sizes.Count == 0)
        //        {
        //            allValidCombinations.Add(new List<Table>(currentCombination));
        //            return;
        //        }

        //        int currentSize = sizes[0];

        //        for (int i = startIndex; i < tables.Count; i++)
        //        {
        //            // Check if the table size matches the current required size
        //            if ((int)tables[i].TableSizeId == currentSize)
        //            {
        //                // Avoid selecting the same table multiple times
        //                if (!currentCombination.Contains(tables[i]))
        //                {
        //                    currentCombination.Add(tables[i]);
        //                    // Recurse for the next size requirement
        //                    BacktrackFindCombinations(tables, sizes.Skip(1).ToList(), i + 1, currentCombination, allValidCombinations);
        //                    // Backtrack
        //                    currentCombination.RemoveAt(currentCombination.Count - 1);
        //                }
        //            }
        //        }
        //    }

        //    private int CalculateTotalProximity(List<Table> combination)
        //    {
        //        int totalProximity = 0;

        //        for (int i = 0; i < combination.Count; i++)
        //        {
        //            for (int j = i + 1; j < combination.Count; j++)
        //            {
        //                totalProximity += CalculateProximityBetweenTables(combination[i], combination[j]);
        //            }
        //        }

        //        return totalProximity;
        //    }

        //    private int CalculateProximityBetweenTables(Table table1, Table table2)
        //    {
        //        int proximity = 0;
        //        var coords1 = DeserializeList(table1.Coordinates);
        //        var coords2 = DeserializeList(table2.Coordinates);

        //        foreach (var cell1 in coords1)
        //        {
        //            foreach (var cell2 in coords2)
        //            {
        //                // Calculate Manhattan distance
        //                int distance = Math.Abs(cell1.Item1 - cell2.Item1) + Math.Abs(cell1.Item2 - cell2.Item2);
        //                if (distance == 1) // Adjacent cells
        //                {
        //                    proximity += 2; // Higher score for adjacency
        //                }
        //                else if (distance == 2) // One cell apart
        //                {
        //                    proximity += 1; // Lower score
        //                }
        //                // No score for distances greater than 2
        //            }
        //        }

        //        return proximity;
        //    }

        private async Task<List<Table>> FindBestTables(List<Table> tables, List<int> requestedSizes)
        {
            List<List<Table>> allValidCombinations = new List<List<Table>>();
            List<Table> currentCombination = new List<Table>();
            int highestProximity = -1;
            List<Table> bestCombination = null;

            // Sort tables by TableSizeId to optimize the combination generation
            var sortedTables = tables.OrderBy(t => t.TableSizeId).ToList();

            // Start backtracking to find all valid combinations
            BacktrackFindCombinations(sortedTables, requestedSizes, 0, currentCombination, allValidCombinations);

            // Iterate through all valid combinations to find the one with the highest proximity
            foreach (var combination in allValidCombinations)
            {
                int totalProximity = CalculateTotalProximity(combination);
                if (totalProximity > highestProximity)
                {
                    highestProximity = totalProximity;
                    bestCombination = combination;
                }
            }

            return bestCombination;
        }

        private void BacktrackFindCombinations(
            List<Table> tables,
            List<int> sizes,
            int startIndex,
            List<Table> currentCombination,
            List<List<Table>> allValidCombinations)
        {
            // If all sizes have been accommodated, add the current combination to the list
            if (sizes.Count == 0)
            {
                allValidCombinations.Add(new List<Table>(currentCombination));
                return;
            }

            int currentSize = sizes[0];

            for (int i = startIndex; i < tables.Count; i++)
            {
                // Check if the table size matches the current required size
                if ((int)tables[i].TableSizeId == currentSize)
                {
                    // Avoid selecting the same table multiple times
                    if (!currentCombination.Contains(tables[i]))
                    {
                        currentCombination.Add(tables[i]);
                        // Recurse for the next size requirement
                        BacktrackFindCombinations(tables, sizes.Skip(1).ToList(), i + 1, currentCombination, allValidCombinations);
                        // Backtrack
                        currentCombination.RemoveAt(currentCombination.Count - 1);
                    }
                }
            }
        }

        private int CalculateTotalProximity(List<Table> combination)
        {
            int totalProximity = 0;

            for (int i = 0; i < combination.Count; i++)
            {
                for (int j = i + 1; j < combination.Count; j++)
                {
                    totalProximity += CalculateProximityBetweenTables(combination[i], combination[j]);
                }
            }

            return totalProximity;
        }

        private int CalculateProximityBetweenTables(Table table1, Table table2)
        {
            int proximity = 0;
            var coords1 = DeserializeList(table1.Coordinates);
            var coords2 = DeserializeList(table2.Coordinates);

            foreach (var cell1 in coords1)
            {
                foreach (var cell2 in coords2)
                {
                    // Calculate Manhattan distance
                    int distance = Math.Abs(cell1.Item1 - cell2.Item1) + Math.Abs(cell1.Item2 - cell2.Item2);
                    if (distance == 1) // Adjacent cells
                    {
                        proximity += 2; // Higher score for adjacency
                    }
                    else if (distance == 2) // One cell apart
                    {
                        proximity += 1; // Lower score
                    }
                    // No score for distances greater than 2
                }
            }

            return proximity;
        }

        private List<int> GetRequiredTableSizes(int numOfPeople)
        {
            // Define possible table sizes (assuming tableSizeId represents the number of people the table can accommodate)
            // For example: 2, 4, 6, 8, 10
            List<int> sizes = new List<int>();

            // Simple greedy algorithm to determine required sizes
            // Start with the largest table size and work downwards
            int remaining = numOfPeople;

            while (remaining > 0)
            {
                if (remaining >= 10)
                {
                    sizes.Add(10);
                    remaining -= 10;
                }
                else if (remaining >= 8)
                {
                    sizes.Add(8);
                    remaining -= 8;
                }
                else if (remaining >= 6)
                {
                    sizes.Add(6);
                    remaining -= 6;
                }
                else if (remaining >= 4)
                {
                    sizes.Add(4);
                    remaining -= 4;
                }
                else
                {
                    sizes.Add(2);
                    remaining -= 2;
                }
            }

            return sizes;
        }

        public async Task<List<Table>> GetAvailableTable(DateTime startTime, DateTime? endTime, bool isPrivate, int? numOfPeople, int pageNumber, int pageSize)
        {
            List<Table> result = new List<Table>();
            try
            {
                var _orderRepository = Resolve<IGenericRepository<Order>>();
                var _configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var conditions = new List<Func<Expression<Func<Order, bool>>>>();

                // !(endTime < r.ReservationDate || r.EndTime < startTime)
                var configurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), 0, 0, null, false, null);
                if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
                {
                    return new List<Table>();
                }
                if (!endTime.HasValue)
                {
                    endTime = startTime.AddHours(double.Parse(configurationDb.Items[0].CurrentValue));
                }

                conditions.Add(() => r => !(endTime < r.ReservationDate || (r.EndTime.HasValue && r.EndTime.Value < startTime || !r.EndTime.HasValue && r.ReservationDate.Value.AddHours(double.Parse(configurationDb.Items[0].CurrentValue)) < startTime))
                                          && r.StatusId != OrderStatus.Cancelled);

                Expression<Func<Order, bool>> expression = r => true; // Default expression to match all

                if (conditions.Count > 0)
                {
                    expression = DynamicLinqBuilder<Order>.BuildExpression(conditions);
                }

                // Get all collided reservations
                var unavailableReservation = await _orderRepository.GetAllDataByExpression(expression, pageNumber, pageSize, null, false, null);
                if (unavailableReservation!.Items.Count > 0)
                {
                    var unavailableReservationIds = unavailableReservation.Items.Select(x => x.OrderId);
                    var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                    var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.OrderId), 0, 0, null, false, r => r.Table.Room);
                    var reservedTableIds = reservedTableDb.Items!.Select(x => x.TableId);
                    var availableTableDb = await _repository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId) && t.Room.IsPrivate == isPrivate && !t.IsDeleted, 0, 0, null, false, t => t.Room);
                    result = availableTableDb.Items;
                }
                else
                {
                    result = (await _repository!.GetAllDataByExpression(t => t.Room.IsPrivate == isPrivate && !t.IsDeleted, 0, 0, null, false, r => r.Room)).Items;
                }
                //result.Result = availableReservation.Items.Select(x => x.Table);
            }
            catch (Exception ex)
            {
                result = new List<Table>();
            }
            return result;
        }

        private List<(int, int)> DeserializeList(string str)
        {
            // Split the string by ';' and convert each substring to a tuple
            if (string.IsNullOrEmpty(str))
            {
                return new List<(int, int)>();
            }
            return str.Split(';')
                      .Select(s => s.Trim('(', ')')) // Remove parentheses
                      .Select(s => s.Split(','))    // Split by ','
                      .Select(arr => (int.Parse(arr[0]), int.Parse(arr[1])))
                      .ToList();
        }

        public async Task<AppActionResult> GetAllTable(int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDb = await _repository.GetAllDataByExpression(t => !t.IsDeleted, pageNumber, pageSize, t => t.TableSize, false, t => t.Room, t => t.TableSize);
                if (tableDb.Items.Count > 0)
                {
                    var data = new List<TableArrangementResponseItem>();
                    var unavailableTableIds = await GetUnavailableTableIds();
                    foreach (var item in tableDb.Items)
                    {
                        var tableResponse = _mapper.Map<TableArrangementResponseItem>(item);
                        List<(int, int)> tableCoordinates = DeserializeList(item.Coordinates);
                        if (tableCoordinates.Count > 0)
                        {
                            tableResponse.Position.X = tableCoordinates.FirstOrDefault().Item1;
                            tableResponse.Position.Y = tableCoordinates.FirstOrDefault().Item2;
                        }

                        tableResponse.TableStatusId = item.TableStatusId;

                        //if (item.TableStatusId == TableStatus.NEW)
                        //{
                        //    tableResponse.TableStatusId = TableStatus.NEW;
                        //}
                        //else if (unavailableTableIds.Contains(tableResponse.Id))
                        //{
                        //    tableResponse.TableStatusId = TableStatus.CURRENTLYUSED;
                        //}
                        //else
                        //{
                        //    tableResponse.TableStatusId = TableStatus.AVAILABLE;
                        //}

                        data.Add(tableResponse);
                    }

                    result.Result = new PagedResult<TableArrangementResponseItem>
                    {
                        Items = data.OrderBy(o => o.Room.IsPrivate).ThenBy(o => o.TableStatusId).ThenBy(o => o.Name).ToList(),
                        TotalPages = tableDb.TotalPages
                    };
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTableCoordinates(List<TableArrangementResponseItem> request, bool? isForce = false)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                var tableSetUpConfig = await congfigurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TABLE_IS_SET_UP), null);
                if (!isForce.HasValue || !isForce.Value)
                {
                    if (tableSetUpConfig != null)
                    {
                        if (tableSetUpConfig.CurrentValue.Equals(SD.DefaultValue.IS_SET_UP))
                        {
                            var checkTableAffect = await CheckUpdateTableCoordinates(request);
                            if (!checkTableAffect.IsSuccess)
                            {
                                return checkTableAffect;
                            }
                        }
                    }
                }
                var tableIds = request.Select(r => r.Id).ToList();
                var tableDb = await _repository.GetAllDataByExpression(r => tableIds.Contains(r.TableId) && !r.IsDeleted, 0, 0, null, false, null);
                if (tableIds.Count != tableDb.Items.Count)
                {
                    return BuildAppActionResultError(result, $"Danh sách chứa id bàn không tồn tại");
                }
                foreach (var table in tableDb.Items)
                {
                    if (table.TableStatusId == TableStatus.NEW)
                    {
                        table.TableStatusId = TableStatus.AVAILABLE;
                    }
                    var inputTable = request.FirstOrDefault(i => i.Id == table.TableId);
                    List<(int, int)> coordinate = new List<(int, int)>();
                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y));
                    if (inputTable.TableSizeId != TableSize.EIGHT && inputTable.TableSizeId != TableSize.TEN)
                    {
                        for (int i = 1; i < (int)inputTable.TableSizeId / 2; i++)
                        {
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + i));
                        }
                    }
                    else
                    {
                        if (inputTable.TableSizeId == TableSize.TEN)
                        {
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 2));
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 3));

                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 2));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 3));
                        }
                        else
                        {
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));

                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                        }
                    }
                    table.TableSizeId = inputTable.TableSizeId;
                    table.Coordinates = ParseListToString(coordinate);
                }
                if (tableSetUpConfig != null)
                {
                    tableSetUpConfig.CurrentValue = SD.DefaultValue.IS_SET_UP;
                    await congfigurationRepository.Update(tableSetUpConfig);
                }
                await _repository.UpdateRange(tableDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        private string ParseListToString(List<(int, int)> list)
        {
            // Convert the list of tuples to a string format "(x,y);(x,y);..."
            return string.Join(";", list.Select(tuple => $"({tuple.Item1},{tuple.Item2})"));
        }

        public async Task<List<Guid>> GetUnavailableTableIds()
        {
            List<Guid> result = new List<Guid>();
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), null);
                var averageTime = double.Parse(configurationDb.CurrentValue);
                var unavailableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.Order.OrderTypeId != OrderType.Delivery
                                                                                                  && t.Order.StatusId != OrderStatus.Cancelled
                                                                                                  && t.Order.MealTime <= currentTime
                                                                                                  && (t.Order.EndTime.HasValue && t.Order.EndTime.Value >= currentTime
                                                                                                      || !t.Order.EndTime.HasValue && t.Order.MealTime.Value.AddHours(averageTime) >= currentTime),
                                                                                                  0, 0, null, false, null);

                result = unavailableDetailDb.Items.DistinctBy(u => u.TableId).Select(u => u.TableId).ToList();
            }
            catch (Exception ex)
            {
                result = new List<Guid>();
            }
            return result;
        }

        public async Task<AppActionResult> GetAllTableRating(int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var tableRatingRepository = Resolve<IGenericRepository<Room>>();
            try
            {
                result.Result = await tableRatingRepository!.GetAllDataByExpression(null, pageNumber, pageSize, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTable(UpdateTableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                //Name validation (if needed)
                if (string.IsNullOrEmpty(dto.TableName) || string.IsNullOrEmpty(dto.DeviceCode) || string.IsNullOrEmpty(dto.DevicePassword))
                {
                    return BuildAppActionResultError(result, "Thông tin không được để trống");
                    return result;
                }

                var tableDb = await _repository.GetById(dto.TableId);
                if (tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {dto.TableId}");
                }

                tableDb.TableName = dto.TableName;
                tableDb.DeviceCode = dto.DeviceCode;

                if (dto.DevicePassword.Length > 0)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.DevicePassword);
                    tableDb.DevicePassword = hashedPassword;
                }
                await _repository.Update(tableDb);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> DeleteTable(Guid id)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDb = await _repository.GetById(id);
                if (tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {id}");
                }
                var tableHasCurrentOrUpcomingReservation = await TableHasCurrentOrUpcomingReservation(id);
                if (tableHasCurrentOrUpcomingReservation)
                {
                    return BuildAppActionResultError(result, $"Bàn đang được sử dụng hoặc có lịch đặt trong tương lai nên không thề xoá");
                }
                tableDb.IsDeleted = true;
                await _repository.Update(tableDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<bool> TableHasCurrentOrUpcomingReservation(Guid id)
        {
            bool tableHasCurrentOrUpcomingReservation = false;
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var configurationRespository = Resolve<IGenericRepository<Configuration>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var averageDiningTimeConfiguration = await configurationRespository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), null);
                double averageDiningTime = averageDiningTimeConfiguration != null ? double.Parse(averageDiningTimeConfiguration.CurrentValue) : 1;
                var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.TableId == id &&
                                                                                          (t.Order.MealTime.Value >= currentTime ||
                                                                                           t.Order.EndTime.Value >= currentTime ||
                                                                                           t.Order.MealTime.Value.AddHours(averageDiningTime) >= currentTime),
                                                                                          0, 0, null, false, null);
                tableHasCurrentOrUpcomingReservation = tableDetailDb.Items.Count() > 0;
            }
            catch (Exception ex)
            {
                tableHasCurrentOrUpcomingReservation = true;
            }
            return tableHasCurrentOrUpcomingReservation;
        }

        public async Task<AppActionResult> UpdateTableAvailabilityAfterPayment(Guid orderId, TableStatus tableStatus)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == orderId, 0, 0, null, false, null);
                if(tableDetailDb.Items.Count > 0)
                {
                    var tableIds = tableDetailDb.Items.Select(t => t.TableId).ToList();
                    var tableDb = await _repository.GetAllDataByExpression(t => tableIds.Contains(t.TableId), 0, 0, null, false, null);
                    tableDb.Items.ForEach(t => t.TableStatusId = tableStatus);
                    await _repository.UpdateRange(tableDb.Items);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTableAvailability(List<Guid> tableIds, TableStatus tableStatus)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDb = await _repository.GetAllDataByExpression(t => tableIds.Contains(t.TableId), 0, 0, null, false, null);
                tableDb.Items.ForEach(t => t.TableStatusId = tableStatus);
                await _repository.UpdateRange(tableDb.Items);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }


        //[Hangfire.Queue("update-tables-availability")]

        public async Task UpdateTableAvailability()
        {
            try
            {
                var utility = Resolve<Utility>();
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var timeToKeepReservationtableResult = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TIME_TO_KEEP_RESERVATION), null);
                double timeToKeepReservationtable = 30;
                if (timeToKeepReservationtableResult != null)
                {
                    timeToKeepReservationtable = double.Parse(timeToKeepReservationtableResult.CurrentValue);
                }

                var orderToSetUsing = await orderRepository.GetAllDataByExpression(o => o.OrderTypeId != OrderType.Delivery 
                                                                                  && o.StatusId == OrderStatus.DepositPaid
                                                                                  && (o.MealTime.Value.AddMinutes(5) >= currentTime || o.MealTime.Value.AddMinutes(5) >= currentTime), 0, 0, null, false, null);
                var orderToSetUsingIds = orderToSetUsing.Items.Select(o => o.OrderId).ToList();
                var tableDetailOrderToSetUsing = await tableDetailRepository.GetAllDataByExpression(t => orderToSetUsingIds.Contains(t.OrderId) && t.Table.TableStatusId == TableStatus.AVAILABLE, 0, 0, null, false, null);
                var toSetUsingTableIds = tableDetailOrderToSetUsing.Items.Select(t => t.TableId).ToList();
                await UpdateTableAvailability(toSetUsingTableIds, TableStatus.CURRENTLYUSED);


                var orderToSetAvailable = await orderRepository.GetAllDataByExpression(o => o.OrderTypeId != OrderType.Delivery 
                                                                                      && (o.StatusId != OrderStatus.DepositPaid && o.MealTime.Value.AddMinutes(30) <= currentTime
                                                                                          || o.StatusId == OrderStatus.Cancelled || o.StatusId == OrderStatus.Completed)
                                                                                      , 0, 0, null, false, null);
                var orderToSetAvailableIds = orderToSetAvailable.Items.Select(o => o.OrderId).ToList();
                var tableDetailOrderToSetAvailable = await tableDetailRepository.GetAllDataByExpression(t => orderToSetAvailableIds.Contains(t.OrderId) && !toSetUsingTableIds.Contains(t.TableId) && t.Table.TableStatusId == TableStatus.CURRENTLYUSED, 0, 0, null, false, null);
                await UpdateTableAvailability(tableDetailOrderToSetAvailable.Items.Select(t => t.TableId).ToList(), TableStatus.AVAILABLE);

                var tableHasBeenUsedDb = await tableDetailRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var tableHasBeenIds = tableHasBeenUsedDb.Items.Select(t => t.TableId);
                var tableHasNotBeenUsedDb = await _repository.GetAllDataByExpression(t => !tableHasBeenIds.Contains(t.TableId), 0, 0, null, false, null);
                tableHasNotBeenUsedDb.Items.ForEach(t => t.TableStatusId = TableStatus.AVAILABLE);
                await _repository.UpdateRange(tableHasNotBeenUsedDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<AppActionResult> CheckUpdateTableCoordinates(List<TableArrangementResponseItem> request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var configurationRespository = Resolve<IGenericRepository<Configuration>>();
                var hashingService = Resolve<IHashingService>();
                var utility = Resolve<Utility>();
                var tableSetUpConfig = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TABLE_IS_SET_UP), null);
                if (tableSetUpConfig != null)
                {
                    if (tableSetUpConfig.CurrentValue.Equals(SD.DefaultValue.IS_SET_UP))
                    {
                        var privateTables = await _repository.GetAllDataByExpression(t => t.Room.IsPrivate, 0, 0, null, false, t => t.Room);
                        StringBuilder sb = new StringBuilder();
                        foreach(var privateTable in privateTables.Items)
                        {
                            var inputTable = request.FirstOrDefault(i => i.Id == privateTable.TableId);
                            List<(int, int)> coordinate = new List<(int, int)>();
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y));
                            if (inputTable.TableSizeId != TableSize.EIGHT && inputTable.TableSizeId != TableSize.TEN)
                            {
                                for (int i = 1; i < (int)inputTable.TableSizeId / 2; i++)
                                {
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + i));
                                }
                            }
                            else
                            {
                                if (inputTable.TableSizeId == TableSize.TEN)
                                {
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 2));
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 3));

                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 2));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 3));
                                }
                                else
                                {
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));

                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                                }
                            }
                            if (!ParseListToString(coordinate).Equals(privateTable.Coordinates))
                            {
                                sb.Append($"{privateTable.TableName}, ");
                            }
                        }

                        if (sb.Length > 0)
                        {
                            sb.Length -= 2;
                            result.IsSuccess = false;
                            result.Messages.Add($"Các bàn riêng tư không thể di chuyển: {sb.ToString()}. Bạn vẫn muốn thay đổi chứ?");
                            return result;
                        }

                        //Check update table reservation
                        var currentTime = utility.GetCurrentDateTimeInTimeZone();
                        ///--
                        var averageDiningTimeConfiguration = await configurationRespository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), null);
                        double averageDiningTime = averageDiningTimeConfiguration != null ? double.Parse(averageDiningTimeConfiguration.CurrentValue) : 1;

                        var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => (t.Order.MealTime >= currentTime
                                                                                                    || (t.Order.EndTime.HasValue && t.Order.EndTime >= currentTime) 
                                                                                                    || (!t.Order.EndTime.HasValue && t.Order.MealTime.Value.AddHours(averageDiningTime) >= currentTime)
                                                                                                    )
                                                                                                && (t.Order.StatusId != OrderStatus.Completed
                                                                                                    || t.Order.StatusId != OrderStatus.Cancelled)
                                                                                               , 0, 0, null, false, t => t.Table);
                        var scheduledTables = tableDetailDb.Items.Select(t => t.Table);
                        var affectedOrderIds = new List<Guid>();
                        foreach (var scheduledTable in scheduledTables)
                        {
                            var inputTable = request.FirstOrDefault(i => i.Id == scheduledTable.TableId);
                            List<(int, int)> coordinate = new List<(int, int)>();
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y));
                            if (inputTable.TableSizeId != TableSize.EIGHT && inputTable.TableSizeId != TableSize.TEN)
                            {
                                for (int i = 1; i < (int)inputTable.TableSizeId / 2; i++)
                                {
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + i));
                                }
                            }
                            else
                            {
                                if (inputTable.TableSizeId == TableSize.TEN)
                                {
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 2));
                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 3));

                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 2));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 3));
                                }
                                else
                                {
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));

                                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                                    coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                                }
                            }
                            if (!ParseListToString(coordinate).Equals(scheduledTable.Coordinates))
                            {
                                affectedOrderIds.AddRange(tableDetailDb.Items.Where(t => t.TableId == scheduledTable.TableId).Select(o => o.OrderId));
                            }
                        }

                        if (affectedOrderIds.Count > 0)
                        {
                            var orderDiningDb = await orderRepository.GetAllDataByExpression(o => affectedOrderIds.Contains(o.OrderId), 0, 0, null, false, o => o.Status,
                                                                                                                                    o => o.OrderType,
                                                                                                                                    o => o.Account,
                                                                                                                                    o => o.Shipper);
                            var decodedAccount = new Dictionary<string, Account>();
                            foreach (var order in orderDiningDb.Items.Where(o => o.Account != null).ToList())
                            {
                                if (decodedAccount.ContainsKey(order.AccountId))
                                {
                                    order.Account = decodedAccount[order.AccountId];
                                }
                                else
                                {
                                    order.Account = hashingService.GetDecodedAccount(order.Account);
                                    decodedAccount.Add(order.AccountId, order.Account);
                                }

                            }
                            List<ReservationTableItemResponse> data = new List<ReservationTableItemResponse>();
                            if (orderDiningDb != null && orderDiningDb.Items.Count > 0)
                            {
                               StringBuilder tableScheduleWarningMessage = new StringBuilder();
                                tableScheduleWarningMessage.Append($"Những đặt bàn sau sẽ bị ảnh hưởng bởi các thay đổi bàn:\n");
                                orderDiningDb.Items = orderDiningDb.Items.OrderByDescending(o => o.MealTime).ToList();

                                foreach (var item in orderDiningDb.Items)
                                {
                                    if (item == null)
                                    {
                                        continue;
                                    }
                                    var reservation = await GetReservationDetailByOrder(item);
                                    if (reservation == null)
                                    {
                                        result.Messages.Add($"Xảy ra lỗi khi truy vấn đặt bàn có id {item.OrderId}");
                                    }
                                    data.Add(reservation);
                                }

                                Dictionary<string, List<DateTime>> tableScheduleWarning = new Dictionary<string, List<DateTime>>();
                                foreach (var order in data)
                                {
                                    foreach (var table in order.Tables)
                                    {
                                        if (tableScheduleWarning.ContainsKey(table.Table.TableName))
                                        {
                                            tableScheduleWarning[table.Table.TableName].Add(order.MealTime.Value);
                                        }
                                        else
                                        {
                                            tableScheduleWarning.Add(table.Table.TableName, new List<DateTime> { order.MealTime.Value });
                                        }
                                    }
                                }

                                foreach (var tableMessage in tableScheduleWarning)
                                {
                                    tableScheduleWarningMessage.Append($"Bàn {tableMessage.Key} vào các khung giờ:\n");
                                    tableScheduleWarningMessage.AppendLine($"{GroupAndFormatDateTimes(tableMessage.Value)}\n");
                                }
                                tableScheduleWarningMessage.Append("Bạn vẫn muốn tiếp tục cập nhật chứ?");
                                result.Messages.Add(tableScheduleWarningMessage.ToString());
                            }
                            
                            result.IsSuccess = false;
                            return result;
                        }                     
                    }
                }
              
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public string GroupAndFormatDateTimes(List<DateTime> dateTimes)
        {
            var groupedByDate = dateTimes
                .GroupBy(dt => dt.Date) // Group by date (ignoring time)
                .OrderBy(g => g.Key);   // Sort by date

            var result = new StringBuilder();

            foreach (var group in groupedByDate)
            {
                var dateString = group.Key.ToString("dd/MM/yyyy");
                var times = group
                    .Select(dt => dt.ToString("HH:mm")) // Format each time as HH:mm
                    .OrderBy(time => time);             // Sort times within the group
                result.AppendLine($"[{dateString}: {string.Join(", ", times)}]");
            }

            return result.ToString().TrimEnd(); // Remove trailing newline
        }

        private async Task<ReservationTableItemResponse> GetReservationDetailByOrder(Order order)
        {
            ReservationTableItemResponse result = null;
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                result = _mapper.Map<ReservationTableItemResponse>(order);
                result.Tables = (await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == order.OrderId, 0, 0, t => t.Table.TableName, false, t => t.Table.TableSize, t => t.Table.Room)).Items;
            }
            catch (Exception ex)
            {
            }
            return result;
        }
    }
}