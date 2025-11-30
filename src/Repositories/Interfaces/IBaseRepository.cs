namespace DevTrails___BankProject.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T?> GetByIdAsync(Guid id);
        Task<List<T>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
