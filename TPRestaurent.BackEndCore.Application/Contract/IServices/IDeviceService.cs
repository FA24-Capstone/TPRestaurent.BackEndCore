using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDeviceService
    {
        Task<AppActionResult> GetAllDevice(int pageNumber, int pageIndex);

        Task<AppActionResult> GetDeviceById(Guid deviceId);

        Task<AppActionResult> CreateNewDevice(DeviceAccessRequest deviceAccess);

        Task<AppActionResult> LoginDevice(LoginDeviceRequestDto loginDeviceRequestDto);
    }
}