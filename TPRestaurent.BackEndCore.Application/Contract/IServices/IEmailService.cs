using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IEmailService
    {
        public void SendEmail(string recipient, string subject, string body);
    }
}
