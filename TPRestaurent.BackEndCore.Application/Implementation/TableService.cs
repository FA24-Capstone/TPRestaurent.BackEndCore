using AutoMapper;
using Castle.Core.Internal;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

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
                //Name validation (if needed)
                if (string.IsNullOrEmpty(dto.TableName)) {
                    result = BuildAppActionResultError(result, "Tên bàn không được để trống");
                    return result;
                }

                if ((int)dto.TableSizeId < 1)
                {
                    result = BuildAppActionResultError(result, "Số ghế ngồi phải lớn hơn 0");
                    return result;
                }

                var tableRatingRepository = Resolve<IGenericRepository<Room>>();
                if((await tableRatingRepository.GetById(dto.TableRatingId) == null))
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy phòng với id {dto.TableRatingId}");
                    return result;
                }

                var newTable = _mapper.Map<Table>(dto);
                newTable.TableId = Guid.NewGuid();
                newTable.IsDeleted = false;
                await _repository.Insert(newTable);
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
                var availableTables = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.RoomId, dto.Quantity, 0, 0);
                if (availableTables.Count == 0)
                {
                    result.Messages.Add("Không tìm thấy bàn cho yêu cầu đặt bàn");
                    return result;
                }
                int[] sizes = { 2, 4, 6, 9, 11 }; // Possible sizes
                int target = dto.Quantity + 2;
                List<List<int>> possibleTableSet = new List<List<int>>();
                await Backtrack(possibleTableSet, new List<int>(), sizes, dto.Quantity, 0, target);

                // Filter out subsets that are not optimal
                possibleTableSet = await FilterOptimalSubsets(possibleTableSet);
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
                    if(tableList != null && tableList.Count > 0)
                    {
                        result.Result = tableList;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message );
            }
            return result;
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

        private async Task<List<List<int>>> FilterOptimalSubsets(List<List<int>> subsets)
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

            return optimalSubsets.OrderBy(o => o.Sum(i => i)).ThenBy(o => o.Count()).ThenBy(o =>
            {
                double mean = o.Average();
                return o.Average(or => Math.Pow(or - mean, 2));
            }).ToList();
        }

        private async Task<List<Table>> FindBestTables(List<Table> tables, List<int> requestedSizes)
        {
            List<Table> bestCombination = null;
            Backtrack(tables, requestedSizes, new List<Table>(), ref bestCombination, 0);
            return bestCombination;
        }

        private void Backtrack(List<Table> tables, List<int> sizes, List<Table> currentTables, ref List<Table> bestCombination, int startIndex)
        {
            // If all requested sizes are used, check if this combination is valid
            if (sizes.Count == 0)
            {
                if (bestCombination == null || currentTables.Count < bestCombination.Count)
                {
                    bestCombination = new List<Table>(currentTables);
                }
                return;
            }

            // Try to find the best table of the current requested size
            int maxProximity = -1;
            Table bestTable = null;

            for (int i = startIndex; i < tables.Count; i++)
            {
                int tableSize = (int)tables[i].TableSizeId; // Each cell represents 2 units

                if (tableSize == sizes[0])
                {
                    int proximity = CalculateProximity(currentTables, tables[i]);

                    // If this table is closer to the existing tables, update the best choice
                    if (proximity > maxProximity)
                    {
                        maxProximity = proximity;
                        bestTable = tables[i];
                    }
                }
            }

            // If a best table is found, proceed with backtracking
            if (bestTable != null)
            {
                currentTables.Add(bestTable);
                tables.Remove(bestTable);
                Backtrack(tables, sizes.Skip(1).ToList(), currentTables, ref bestCombination, startIndex + 1);
                currentTables.RemoveAt(currentTables.Count - 1); // Backtrack step
            }
        }

        private int CalculateProximity(List<Table> currentTables, Table newTable)
        {
            int proximity = 0;

            foreach (var currentTable in currentTables)
            {
                foreach (var cell in DeserializeList(currentTable.Coordinates))
                {
                    foreach (var newCell in DeserializeList(newTable.Coordinates))
                    {
                        // Check if cells are adjacent (horizontally or vertically)
                        if (Math.Abs(cell.Item1 - newCell.Item1) + Math.Abs(cell.Item2 - newCell.Item2) == 1)
                        {
                            proximity++;
                        }
                    }
                }
            }
            return proximity;
        }

        public async Task<List<Table>> GetAvailableTable(DateTime startTime, DateTime? endTime, Guid roomId, int? numOfPeople, int pageNumber, int pageSize)
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
                    var availableTableDb = await _repository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId) && t.RoomId == roomId, 0, 0, null, false, t => t.Room);
                    result = availableTableDb.Items;
                }
                else
                {
                    result = (await _repository!.GetAllDataByExpression(t => t.RoomId == roomId, 0, 0, null, false, r => r.Room)).Items;
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
                result.Result = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, null, false, t => t.Room, t => t.TableSize);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        //public async Task<AppActionResult> GetTableById(Guid TableId)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        result.Result = await _repository.GetByExpression(null, t => t.TableRating);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        //public async Task<AppActionResult> UpdateTable(Guid TableId, TableDto dto)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        var tableDb = await _repository.GetById(TableId);
        //        if(tableDb == null)
        //        {
        //            result = BuildAppActionResultError(result, $"Không tìm thấy bàn với id {TableId}");
        //            return result;
        //        } 

        //        //Name validation (if needed)
        //        if (string.IsNullOrEmpty(dto.TableName))
        //        {
        //            result = BuildAppActionResultError(result, "Tên bàn không được để trống");
        //            return result;
        //        }

        //        if ((int)dto.TableSizeId < 1)
        //        {
        //            result = BuildAppActionResultError(result, "Số ghế ngồi phải lớn hơn 0");
        //            return result;
        //        }

        //        var tableRatingRepository = Resolve<IGenericRepository<Room>>();
        //        if ((await tableRatingRepository.GetById(dto.TableRatingId) == null))
        //        {
        //            result = BuildAppActionResultError(result, $"Không tìm thấy phân loại bàn với id {dto.TableRatingId}");
        //            return result;
        //        }

        //        tableDb.TableName = dto.TableName;  
        //        tableDb.TableSizeId = dto.TableSizeId;  
        //        tableDb.TableRatingId = dto.TableRatingId;

        //        await _repository.Update(tableDb);
        //        await _unitOfWork.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}
    }
}
