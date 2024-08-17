using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IDeviceService
    {
        Task<AppActionResult> GetDeviceById(Guid deviceId);
        Task<AppActionResult> CreateNewDevice(DeviceAccessRequest deviceAccess);
        Task<AppActionResult> LoginDevice(LoginDeviceRequestDto loginDeviceRequestDto);
    }
}
