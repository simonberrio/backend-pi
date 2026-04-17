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
        Task<EventResponseDto> CreateEventAsync(EventDto model);
        Task<EventResponseDto> DeleteEventAsync(int id);
        Task<List<EventResponseDto>> GetEventsAsync(EventFilterDto filter);
        Task<EventResponseDto> UpdateEventAsync(EventDto model);
    }
}
