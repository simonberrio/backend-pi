using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IReactionRepository
    {
        Task<Reaction> CreateAsync(Reaction entity);
        Task<Reaction> DeleteAsync(Reaction entity);
        IQueryable<Reaction> GetQueryable();
        Task<Reaction> UpdateAsync(Reaction entity);

    }
}
