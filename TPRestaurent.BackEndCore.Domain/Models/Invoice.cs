using System.ComponentModel.DataAnnotations;
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