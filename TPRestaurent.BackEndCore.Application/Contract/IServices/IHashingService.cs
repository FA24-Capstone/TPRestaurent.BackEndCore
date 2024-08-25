using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IHashingService
    {
        public string Hashing(string password, string key);
        public string DeHashing(string password, string key);
    }
}
