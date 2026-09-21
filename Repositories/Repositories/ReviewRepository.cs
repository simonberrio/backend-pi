using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class ReviewRepository(AppDbContext context) : IReviewRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Review> CreateAsync(Review entity)
        {
            EntityEntry<Review> response = _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<Review> DeleteAsync(Review entity)
        {
            EntityEntry<Review> response = _context.Reviews.Remove(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<Review> GetQueryable()
        {
            return _context.Reviews.AsQueryable();
        }

        public async Task<Review> UpdateAsync(Review entity)
        {
            EntityEntry<Review> response = _context.Reviews.Update(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }
    }
}
