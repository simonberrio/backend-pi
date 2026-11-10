using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;

namespace Repositories.Repositories
{
    public class OrganizerPaymentSettingsRepository(AppDbContext context) : IOrganizerPaymentSettingsRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<OrganizerPaymentSettings> CreateAsync(OrganizerPaymentSettings entity)
        {
            EntityEntry<OrganizerPaymentSettings> response = _context.OrganizerPaymentSettings.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<OrganizerPaymentSettings> GetQueryable()
        {
            return _context.OrganizerPaymentSettings.AsQueryable();
        }

        public async Task<OrganizerPaymentSettings> UpdateAsync(OrganizerPaymentSettings entity)
        {
            OrganizerPaymentSettings current = await _context.OrganizerPaymentSettings.FirstAsync(x => x.Id == entity.Id);
            _context.Entry(current).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return current;
        }
    }
}
