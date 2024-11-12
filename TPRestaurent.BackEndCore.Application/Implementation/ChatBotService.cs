using Castle.DynamicProxy.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ChatBotService : GenericBackendService, IChatBotService
    {
        private IGenericRepository<Account> _accountRepository;
        private IGenericRepository<Dish> _dishRepository;
        private IOpenAIClient _client;
        public ChatBotService(IGenericRepository<Account> accountRepository,
                              IGenericRepository<Dish> dishRepository,
                              IOpenAIClient client,
                              IServiceProvider service) : base(service)
        {
            _accountRepository = accountRepository;
            _dishRepository = dishRepository;
            _client = client;
        }

        public async Task<AppActionResult> ResponseCustomer(string customerId, string messageText)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                result.Result = await _client.CreateChatCompletion(messageText);
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
