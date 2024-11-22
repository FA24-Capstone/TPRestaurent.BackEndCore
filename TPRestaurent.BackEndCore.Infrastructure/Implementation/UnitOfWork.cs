using TPRestaurent.BackEndCore.Application;
using TPRestaurent.BackEndCore.Domain.Data;
using System;
using System.Transactions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Infrastructure.Implementation
{
    
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDBContext _context;

        public UnitOfWork(IDBContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task ExecuteInTransaction(Func<Task> operation)
        {
            AppActionResult result = new AppActionResult();
            var strategy = _context.Database.CreateExecutionStrategy();

            // Execute the operation with retry logic in case of transient errors
            await strategy.ExecuteAsync(async () =>
            {
                // Begin a transaction on the DbContext
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Execute the passed-in operation (database operations)
                    await operation();

                    // Commit the transaction if everything is successful
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // If an error occurs, log it or handle it, and then roll back the transaction
                    Console.WriteLine($"Transaction failed: {ex.Message}");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        
    }
    }
}