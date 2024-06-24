using TPRestaurent.BackEndCore.Application;
using TPRestaurent.BackEndCore.Domain.Data;

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
    }
}