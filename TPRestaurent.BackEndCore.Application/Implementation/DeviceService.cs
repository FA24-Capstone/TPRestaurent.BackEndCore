using AutoMapper;
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

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class DeviceService : GenericBackendService, IDeviceService
    {
        private readonly IGenericRepository<Device> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenDto _tokenDto;

        public DeviceService(
            IGenericRepository<Device> repository,
            IMapper mapper, IUnitOfWork unitOfWork, 
            IServiceProvider service
            ) : base(service)
        {
            _repository = repository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _tokenDto = new TokenDto();
        }
        
        public async Task<AppActionResult> CreateNewDevice(DeviceAccessRequest deviceAccess)
        {
            var result = new AppActionResult();
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(deviceAccess.DevicePassword);
                var newDeviceDb = new Device
                {
                    DeviceId = Guid.NewGuid(),
                    DeviceCode = deviceAccess.DeviceCode,
                    DevicePassword = hashedPassword,
                    TableId = deviceAccess.TableId,
                };
                await _repository.Insert(newDeviceDb);  
                await _unitOfWork.SaveChangesAsync();
                result.Result = newDeviceDb; 
            }
            catch (Exception ex) 
            { 
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetDeviceById(Guid deviceId)
        {
            var result = new AppActionResult();
            try
            {
                var deviceDb = await _repository.GetByExpression(p => p.DeviceId == deviceId, p => p.Table!);
                if (deviceDb == null)
                {
                    result = BuildAppActionResultError(result, $"Thiết bị với id {deviceId} không tồn tại");
                }
                result.Result = deviceDb;   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;  
        }

        public async Task<AppActionResult> LoginDevice(LoginDeviceRequestDto loginDeviceRequestDto)
        {
            var result = new AppActionResult();
            try
            {
                var device = await _repository!.GetByExpression(p => p.DeviceCode == loginDeviceRequestDto.DeviceCode, null);
                if (device == null)
                    result = BuildAppActionResultError(result, $" Thiết bị với {loginDeviceRequestDto.DeviceCode} này không tồn tại trong hệ thống");

                var verifyPasswordHashed = BCrypt.Net.BCrypt.Verify(loginDeviceRequestDto.Password, device!.DevicePassword);
                var passwordSignIn = await _repository.GetByExpression(p => p.DeviceCode == loginDeviceRequestDto.DeviceCode && verifyPasswordHashed == true);
                if (passwordSignIn == null) result = BuildAppActionResultError(result, "Đăng nhâp thất bại");
                if (!BuildAppActionResultIsError(result)) result = await LoginDefaultDevice(loginDeviceRequestDto.DeviceCode, device);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<AppActionResult> LoginDefaultDevice(string deviceCode, Device? device)
        {
            var result = new AppActionResult();

            var jwtService = Resolve<IJwtService>();
            var utility = Resolve<Utility>();
            var token = await jwtService!.GenerateAccessTokenForDevice(new LoginDeviceRequestDto { DeviceCode = deviceCode });

            _tokenDto.Token = token;
            _tokenDto.DeviceResponse = _mapper.Map<DeviceResponse>(device);
            _tokenDto.MainRole = "DEVICE";

            _tokenDto.Account.MainRole = _tokenDto.MainRole;
            result.Result = _tokenDto;
            await _unitOfWork.SaveChangesAsync();

            return result;
        }
    }
}
