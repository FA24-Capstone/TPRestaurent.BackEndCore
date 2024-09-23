using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OrderSessionStatus
    {
        PreOrder,
        Confirmed,
        Processing,
        Completed,
        Cancelled
    }
}
