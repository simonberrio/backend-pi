using Dtos;

namespace Services.IService
{
    public interface IEventParticipantService
    {
        Task<EventParticipantDto> ApproveOrRejectParticipant(ManageParticipantDto model);
        Task<EventParticipantDto> CancelRegistrationAsync(RegistrationDto cancelRegistrationDto);
        Task<List<EventParticipantDto>> GetParticipantsByEventIdAsync(int eventId);
        Task<List<EventParticipantDto>> GetPendingRequestsAsync(int eventId);
        Task<EventParticipantDto> RegisterToEventAsync(RegistrationDto registrationDto);
    }
}
