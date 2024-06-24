using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TPRestaurent.BackEndCore.Domain.Data
{
    public interface IDBContext
    {
        DbSet<T> Set<T>() where T : class;

        EntityEntry<T> Entry<T>(T entity) where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}