using Dtos;

namespace Services.IService
{
    public interface IEventParticipantService
    {
        Task<EventParticipantDto> CancelRegistrationAsync(RegistrationDto cancelRegistrationDto);
        Task<List<EventParticipantDto>> GetParticipantsByEventIdAsync(int? eventId);
        Task<EventParticipantDto> RegisterToEventAsync(RegistrationDto registrationDto);
    }
}
