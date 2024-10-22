using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DeviceReponse
    {
        public Guid TableId { get; set; }
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
    }
}
