using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;

namespace Repositories.Repositories
{
    public class EventRepository(AppDbContext context) : IEventRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Event> CreateAsync(Event entity)
        {
            EntityEntry<Event> response = _context.Events.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<Event> DeleteAsync(Event entity)
        {
            EntityEntry<Event> response = _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<Event> GetQueryable()
        {
            return _context.Events.AsQueryable();
        }

        public async Task<Event> UpdateAsync(Event entity)
        {
            EntityEntry<Event> response = _context.Events.Update(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }
    }
}
