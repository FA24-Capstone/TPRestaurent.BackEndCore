using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DeviceAccessRequest
    {
        public string DeviceCode { get; set; }
        public string DevicePassword { get; set; }
    }
}
