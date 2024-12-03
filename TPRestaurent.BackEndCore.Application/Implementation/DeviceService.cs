using AutoMapper;
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
        private readonly IGenericRepository<Table> _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenDto _tokenDto;

        public DeviceService(
            IGenericRepository<Table> repository,
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
                var tableDb = await _repository.GetByExpression(p => p.TableId == deviceAccess.TableId);
                if (tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tồn tại bàn với id {deviceAccess.TableId}");
                }

                if (!string.IsNullOrEmpty(tableDb.DeviceCode) && !string.IsNullOrEmpty(tableDb.DevicePassword))
                {
                    return BuildAppActionResultError(result, $"Thiết bị với code {tableDb.DeviceCode} đã tồn tại");
                }

                tableDb.DeviceCode = deviceAccess.DeviceCode;
                tableDb.DevicePassword = deviceAccess.DevicePassword;

                await _repository.Update(tableDb);
                await _unitOfWork.SaveChangesAsync();
                result.Result = _mapper.Map<DeviceResponse>(tableDb);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllDevice(int pageNumber, int pageIndex)
        {
            var result = new AppActionResult();
            try
            {
                var deviceDb = await _repository.GetAllDataByExpression(null, pageNumber, pageIndex, null, false, null);
                result.Result = new PagedResult<DeviceResponse>
                {
                    Items = _mapper.Map<List<DeviceResponse>>(deviceDb.Items),
                    TotalPages = deviceDb.TotalPages
                };
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
                var deviceDb = await _repository.GetById(deviceId);
                if (deviceDb == null)
                {
                    BuildAppActionResultError(result, $"Thiết bị với id {deviceId} không tồn tại");
                }
                result.Result = _mapper.Map<DeviceResponse>(deviceDb);
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
                var device = await _repository!.GetByExpression(p => p.DeviceCode.Equals(loginDeviceRequestDto.DeviceCode), null);
                if (device == null)
                {
                    return BuildAppActionResultError(result, $" Thiết bị với {loginDeviceRequestDto.DeviceCode} này không tồn tại trong hệ thống");
                }
                var verifyPasswordHashed = BCrypt.Net.BCrypt.Verify(loginDeviceRequestDto.Password, device.DevicePassword);
                var passwordSignIn = await _repository.GetByExpression(p => p.DeviceCode == loginDeviceRequestDto.DeviceCode);
                if (passwordSignIn == null || !verifyPasswordHashed) return BuildAppActionResultError(result, "Đăng nhâp thất bại");
                if (!BuildAppActionResultIsError(result)) result = await LoginDefaultDevice(loginDeviceRequestDto.DeviceCode, device);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<AppActionResult> LoginDefaultDevice(string deviceCode, Table? device)
        {
            var result = new AppActionResult();

            var jwtService = Resolve<IJwtService>();
            var utility = Resolve<Utility>();
            var token = await jwtService!.GenerateAccessTokenForDevice(new LoginDeviceRequestDto { DeviceCode = deviceCode });

            _tokenDto.Token = token;
            _tokenDto.DeviceResponse = _mapper.Map<DeviceResponse>(device);
            _tokenDto.DeviceResponse.TableName = device.TableName;
            _tokenDto.MainRole = "DEVICE";

            _tokenDto.DeviceResponse.MainRole = _tokenDto.MainRole;
            result.Result = _tokenDto;
            await _unitOfWork.SaveChangesAsync();

            return result;
        }
    }
}