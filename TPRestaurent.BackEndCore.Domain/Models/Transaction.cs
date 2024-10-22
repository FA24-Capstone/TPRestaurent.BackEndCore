using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Models.EnumModels;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Transaction
    {
        [Key] 
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PaidDate { get; set; }
        public Enums.PaymentMethod PaymentMethodId { get; set; }
        [ForeignKey(nameof(PaymentMethodId))]
        public PaymentMethod? PaymentMethod { get; set; }
        public Enums.TransationStatus TransationStatusId { get; set; }
        [ForeignKey(nameof(TransationStatusId))]
        public EnumModels.TransationStatus TransationStatus { get; set; }
        public Enums.TransactionType TransactionTypeId { get; set; }
        [ForeignKey(nameof(TransactionTypeId))]
        public EnumModels.TransactionType TransactionType { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
        public string? AccountId { get; set; }
        [ForeignKey(nameof(AccountId))]
        public Account? Account { get; set; }

    }
}
