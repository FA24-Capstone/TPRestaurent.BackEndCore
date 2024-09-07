using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        TableAssigned = 3,
        Paid = 4,
        Dining = 5, 
        Completed = 6,
        Cancelled = 7,
        Delivering = 8
    }
}
