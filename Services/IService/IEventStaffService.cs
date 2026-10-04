using Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IEventStaffService
    {
        Task<EventStaffResponseDto> AddStaffAsync(EventStaffDto model);
        Task<List<EventStaffResponseDto>> GetStaffByEventIdAsync(int eventId);
        Task RemoveStaffAsync(EventStaffDto model);
    }
}
