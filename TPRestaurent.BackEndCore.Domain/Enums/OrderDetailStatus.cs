using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum OrderDetailStatus
    {
        Reserved = 1,
        Unchecked = 2,
        Processing = 3,
        ReadyToServe = 4,
        Cancelled = 5,
        LateWarning = 6
    }
}
