using Dtos;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IEventService
    {
        Task<EventDto> CreateEventAsync(EventDto model);
    }
}
