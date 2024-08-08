using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    internal class DeviceService : GenericBackendService, IDeviceService
    {
        private readonly IGenericRepository<Device> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public DeviceService(IGenericRepository<Device> repository, IMapper mapper, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AccessDevice(DeviceAccessRequest dto)
        {
            AppActionResult result = new AppActionResult(); 
            try
            {
                if(!string.IsNullOrEmpty(dto.DeviceCode) || !string.IsNullOrEmpty(dto.DevicePassword))
                {
                    result = BuildAppActionResultError(result, $"Không được để trống mã code hoặc mật khẩu");
                    return result;
                }

                var deviceDb = await _repository.GetByExpression(d => d.DeviceCode.Equals(dto.DeviceCode) && d.DevicePassword.Equals(dto.DevicePassword), d => d.Table);
                if (deviceDb != null) 
                { 
                    result.Result = deviceDb;
                }
            }
            catch (Exception ex) 
            { 
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
