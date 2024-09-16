using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Common.DTO.Request
{
    public class CustomerInforRequest
    {
        public string CustomerInfoAddressName { get; set; }
        public bool IsCurrentUsed { get; set; }
        public string AccountId { get; set; }
    }
}
