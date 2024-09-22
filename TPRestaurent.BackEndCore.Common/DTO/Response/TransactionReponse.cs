using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TPRestaurent.BackEndCore.Domain.Models;
using Transaction = TPRestaurent.BackEndCore.Domain.Models.Transaction;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TransactionReponse
    {
        public Transaction Transaction { get; set; }
        public ReservationReponse? Order { get; set; }
    }
}
