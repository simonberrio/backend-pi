using Dtos;
using Microsoft.AspNetCore.Http;

namespace Services.IService
{
    public interface IEventService
    {
        Task<EventResponseDto> CreateEventAsync(EventDto model);
        Task<EventResponseDto> DeleteEventAsync(int id);
        Task<List<EventResponseDto>> GetEventsAsync(EventFilterDto filter);
        Task<List<EventResponseDto>> GetEventsIAmRegistered();
        Task<EventResponseDto> UpdateEventAsync(EventDto model);
        Task<EventResponseDto> UploadImageAsync(int eventId, IFormFile formFile);
    }
}
