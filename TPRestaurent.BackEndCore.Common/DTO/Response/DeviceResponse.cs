using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class DeviceResponse
    {
        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;
        public Guid TableId { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public string MainRole { get; set; }
    }
}
