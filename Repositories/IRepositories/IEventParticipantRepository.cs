using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IEventParticipantRepository
    {
        Task<EventParticipant> CreateAsync(EventParticipant entity);
        Task<EventParticipant> DeleteAsync(EventParticipant entity);
        Task<EventParticipant?> GetByIdAsync(int id);
        IQueryable<EventParticipant> GetQueryable();
        Task<EventParticipant> UpdateAsync(EventParticipant entity);
    }
}
