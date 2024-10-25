using AutoMapper;
using Castle.Core.Internal;
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
