using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Enums
{
    public enum TransactionType
    {
        Deposit = 1,
        Order = 2,
        CreditStore = 3,
        Refund = 4
    }
}
