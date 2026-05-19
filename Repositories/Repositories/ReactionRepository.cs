using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.IRepositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class ReactionRepository(AppDbContext context) : IReactionRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Reaction> CreateAsync(Reaction entity)
        {
            EntityEntry<Reaction> response = _context.Reactions.Add(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public async Task<Reaction> DeleteAsync(Reaction entity)
        {
            EntityEntry<Reaction> response = _context.Reactions.Remove(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }

        public IQueryable<Reaction> GetQueryable()
        {
            return _context.Reactions.AsQueryable();
        }

        public async Task<Reaction> UpdateAsync(Reaction entity)
        {
            EntityEntry<Reaction> response = _context.Reactions.Update(entity);
            await _context.SaveChangesAsync();
            return response.Entity;
        }
    }
}
