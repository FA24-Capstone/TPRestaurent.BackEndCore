using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class UpdateTableDto
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; } = null!;
        public string DeviceCode { get; set; }
        public string DevicePassword { get; set; }
    }
}
