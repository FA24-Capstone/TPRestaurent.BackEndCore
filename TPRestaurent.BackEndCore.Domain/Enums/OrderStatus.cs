using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OrderStatus
    {
        TableAssigned = 1,
        DepositPaid = 2,
        Dining = 3, 
        Pending = 4,
        Processing = 5,
        ReadyForDelivery = 6,
        Delivering = 7,
        Completed = 8,
        Cancelled = 9,
    }
}
