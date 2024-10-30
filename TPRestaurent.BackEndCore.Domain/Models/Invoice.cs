using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Domain.Models
{
    public class Invoice
    {
        [Key]
        public Guid InvoiceId { get; set; }
        public string OrderDetailJson { get; set; }
        public string pdfLink { get; set; }
    }
}
