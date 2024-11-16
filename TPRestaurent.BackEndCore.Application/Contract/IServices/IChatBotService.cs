using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IChatBotService
    {
        Task<AppActionResult> ResponseCustomer(string customerId, string messageText);
    }
}
