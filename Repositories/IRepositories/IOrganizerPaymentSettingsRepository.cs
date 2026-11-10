using Repositories.Models;

namespace Repositories.IRepositories
{
    public interface IOrganizerPaymentSettingsRepository
    {
        Task<OrganizerPaymentSettings> CreateAsync(OrganizerPaymentSettings entity);
        IQueryable<OrganizerPaymentSettings> GetQueryable();
        Task<OrganizerPaymentSettings> UpdateAsync(OrganizerPaymentSettings entity);
    }
}
