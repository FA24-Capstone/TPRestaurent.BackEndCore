﻿using Transaction = TPRestaurent.BackEndCore.Domain.Models.Transaction;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class TransactionReponse
    {
        public Transaction Transaction { get; set; }
        public OrderWithDetailReponse? Order { get; set; }
    }
}