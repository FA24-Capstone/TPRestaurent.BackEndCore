using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OpenAIClient : IOpenAIClient
    {
        private readonly ChatClient _client;

        public OpenAIClient()
        {
            IConfiguration config = new ConfigurationBuilder()
                           .SetBasePath(Directory.GetCurrentDirectory())
                           .AddJsonFile("appsettings.json", true, true)
                           .Build();
            string apiKey = config["OpenAI:Key"];
            _client = new ChatClient(
                model: "gpt-4o-mini", 
                apiKey: apiKey
                );
        }

        public async Task<string> CreateChatCompletion(string userMessage)
        {
            string response = "";
            try
            {
                ChatCompletion completion = _client.CompleteChat(userMessage);
                response = completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            return response;
        }
    }
}
