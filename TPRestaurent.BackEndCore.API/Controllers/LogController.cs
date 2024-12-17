using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    public class LogController 
    {
        private readonly ITransactionService _transactionService;
        public LogController(ITransactionService transactionService) 
        {
            _transactionService = transactionService; 
        }

        [HttpGet("read-log")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<List<LogDto>> ReadLogTest()
        {

            return await Logger.ReadLogs();
        }

        [HttpGet("read-log/{id}")]
        [TokenValidationMiddleware(Permission.ADMIN)]

        public async Task<LogDto> ReadLogTest(Guid id)
        {
            return await Logger.ReadLogById(id);
        }

        [HttpPost("add-log")]
        public void Write([FromBody]LogDto dto)
        {
            Logger.WriteLog(dto);
        }

        [HttpPost("delete-log")]
        public async Task Delete()
        {
            await _transactionService.HashingData();
            Logger.DeleteLogs();
        }
    }
}
