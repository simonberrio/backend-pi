using Repositories;
using Repositories.IRepositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class EventRepository(AppDbContext context) : IEventRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Event> CreateAsync(Event entity)
        {
            _context.Events.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
