using Repositories.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IEventStaffRepository
    {
        Task<EventStaff> CreateAsync(EventStaff entity);
        Task<EventStaff> DeleteAsync(EventStaff entity);
        IQueryable<EventStaff> GetQueryable();
    }
}
