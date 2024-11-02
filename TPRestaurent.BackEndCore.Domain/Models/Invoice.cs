using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Invoice
    {
        [Key]
        public Guid InvoiceId { get; set; }
        public string OrderDetailJson { get; set; }
        public OrderType OrderTypeId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string pdfLink { get; set; }
        public Guid OrderId { get; set; }
    }
}
