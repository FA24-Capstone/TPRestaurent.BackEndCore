using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class LoginDeviceRequestDto
    {
        public string DeviceCode { get; set; } = null!;
        public string Password = null!; 
    }
}
