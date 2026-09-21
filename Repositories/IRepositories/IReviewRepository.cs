using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateAsync(Review entity);
        Task<Review> DeleteAsync(Review entity);
        IQueryable<Review> GetQueryable();
        Task<Review> UpdateAsync(Review entity);
    }
}
