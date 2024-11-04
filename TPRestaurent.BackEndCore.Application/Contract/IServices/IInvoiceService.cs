using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IInvoiceService
    {
        public Task GenerateInvoice();
        public Task<AppActionResult> GetAllInvoice(InvoiceFilterRequest dto);
        public Task<AppActionResult> GenerateGeneralInvoice(InvoiceFilterRequest dto);
        public Task<AppActionResult> GetInvoiceById(Guid id);
    }
}
