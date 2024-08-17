using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class DeviceAccessRequest
    {
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
        public Guid TableId { get; set; }
    }
}
