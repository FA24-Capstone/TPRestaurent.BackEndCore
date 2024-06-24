namespace TPRestaurent.BackEndCore.Application;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}