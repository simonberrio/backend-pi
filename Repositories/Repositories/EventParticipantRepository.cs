using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;

namespace Repositories.Repositories
{
    public class EventParticipantRepository(AppDbContext context) : IEventParticipantRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<EventParticipant> CreateAsync(EventParticipant entity)
        {
            EntityEntry<EventParticipant> response = _context.EventParticipants.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<EventParticipant> DeleteAsync(EventParticipant entity)
        {
            EntityEntry<EventParticipant> response = _context.EventParticipants.Remove(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<EventParticipant?> GetByIdAsync(int id)
        {
            return await _context.EventParticipants.FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<EventParticipant> GetQueryable()
        {
            return _context.EventParticipants.AsQueryable();
        }

        public async Task<EventParticipant> UpdateAsync(EventParticipant entity)
        {
            EventParticipant current = _context.EventParticipants.FirstOrDefault(x => x.Id == entity.Id);
            _context.Entry(current).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return _context.EventParticipants.Include(x => x.User).Include(x => x.Event).FirstOrDefault(x => x.Id == entity.Id);
        }
    }
}
