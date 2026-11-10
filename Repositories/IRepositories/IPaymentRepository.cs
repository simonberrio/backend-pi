using Repositories.Models;

namespace Repositories.IRepositories
{
    public interface IPaymentRepository
    {
        Task<Payment> CreateAsync(Payment entity);
        IQueryable<Payment> GetQueryable();
        Task<Payment> UpdateAsync(Payment entity);
    }
}
