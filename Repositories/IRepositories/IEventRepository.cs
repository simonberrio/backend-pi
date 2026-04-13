using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IEventRepository
    {
        Task<Event> CreateAsync(Event entity);
        Task DeleteAsync(Event entity);
        Task<Event?> GetByIdAsync(int id);
        IQueryable<Event> GetQueryable();
        Task UpdateAsync(Event entity);
    }
}
