using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class EventStaffRepository(AppDbContext context) : IEventStaffRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<EventStaff> CreateAsync(EventStaff entity)
        {
            EntityEntry<EventStaff> response = _context.EventStaff.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<EventStaff> DeleteAsync(EventStaff entity)
        {
            EntityEntry<EventStaff> response = _context.EventStaff.Remove(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<EventStaff> GetQueryable()
        {
            return _context.EventStaff.AsQueryable();
        }
    }
}
