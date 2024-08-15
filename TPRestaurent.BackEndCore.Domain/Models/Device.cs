using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Device
    {
        public Guid DeviceId { get; set; }
        public string DeviceCode { get; set; } = null!;
        public string DevicePassword { get; set; } = null!;     
        public Guid TableId { get; set; }
        [ForeignKey(nameof(TableId))]
        public Table? Table { get; set; }
        public string? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }
    }
}
