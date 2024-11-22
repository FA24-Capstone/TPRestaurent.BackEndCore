namespace TPRestaurent.BackEndCore.Application;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    public Task ExecuteInTransaction(Func<Task> operation);

}