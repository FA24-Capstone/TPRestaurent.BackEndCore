using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IResponseCacheService
    {
        Task SetCacheResponseAsync(string cacheKey, object response, TimeSpan timeOut);
        Task<string> GetCacheResponseAsync(string cacheKey);
        Task RemoveCacheResponseAsync(string pattern);
        Task EnqueueResponseAsync<T>(string queueName, T obj) where T : class;
        Task<T> DequeueResponseAsync<T>(string queueName) where T : class;
    }
}
