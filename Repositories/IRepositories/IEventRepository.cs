using Repositories.Models;

namespace Repositories.IRepositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event entity);
        Task<Event> DeleteAsync(Event entity);
        IQueryable<Event> GetQueryable();
        Task<Event> UpdateAsync(Event entity);
    }
}
