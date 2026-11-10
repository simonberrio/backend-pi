using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;

namespace Repositories.Repositories
{
    public class PaymentRepository(AppDbContext context) : IPaymentRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Payment> CreateAsync(Payment entity)
        {
            EntityEntry<Payment> response = _context.Payments.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<Payment> GetQueryable()
        {
            return _context.Payments.AsQueryable();
        }

        public async Task<Payment> UpdateAsync(Payment entity)
        {
            Payment current = await _context.Payments.FirstAsync(x => x.Id == entity.Id);
            _context.Entry(current).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return current;
        }
    }
}
