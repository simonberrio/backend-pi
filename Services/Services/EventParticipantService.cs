using Dtos;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;

namespace Services.Services
{
    public class EventParticipantService(IEventRepository eventRepository,
        IEventParticipantRepository eventParticipantRepository,
        IUserService userService) : IEventParticipantService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository = eventParticipantRepository;
        private readonly IUserService _userService = userService;

        public async Task<EventParticipantDto> CancelRegistrationAsync(RegistrationDto registrationDto)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            EventParticipant entity = await _eventParticipantRepository.GetQueryable().Where(x => x.UserId == user.Id &&
                x.EventId == registrationDto.EventId).FirstOrDefaultAsync()
                ?? throw new Exception("No estás registrado en este evento");

            entity.Status = ParticipantStatusEnums.Cancelled;
            entity.CancellationReason = registrationDto.CancellationReason;

            EventParticipant response = await _eventParticipantRepository.UpdateAsync(entity);
            return new EventParticipantDto
            {
                Id = response.Id,
                UserId = user.Id,
                UserName = user.UserName,
                UserFirstName = response.User.FirstName,
                UserLastName = response.User.LastName,
                EventId = response.EventId,
                Event = new EventResponseDto
                {
                    Id = response.Event.Id,
                    Name = response.Event.Name,
                    Description = response.Event.Description,
                    StartDate = response.Event.StartDate,
                    EndDate = response.Event.EndDate,
                    MaxParticipants = response.Event.MaxParticipants,
                    IsPublic = response.Event.IsPublic
                },
                RegistrationDate = response.RegistrationDate,
                Status = response.Status,
                ConfirmationDate = response.ConfirmationDate,
                CancellationReason = response.CancellationReason
            };
        }

        public async Task<List<EventParticipantDto>> GetParticipantsByEventIdAsync(int? eventId)
        {
            List<EventParticipantDto> participants = await _eventParticipantRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.Status == ParticipantStatusEnums.Approved)
                .Select(x => new EventParticipantDto {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    EventId = x.EventId,
                    Event = new EventResponseDto
                    {
                        Id = x.Event.Id,
                        Name = x.Event.Name,
                        Description = x.Event.Description,
                        StartDate = x.Event.StartDate,
                        EndDate = x.Event.EndDate,
                        MaxParticipants = x.Event.MaxParticipants,
                        IsPublic = x.Event.IsPublic
                    },
                    RegistrationDate = x.RegistrationDate,
                    Status = x.Status,
                    ConfirmationDate = x.ConfirmationDate,
                    CancellationReason = x.CancellationReason
                })
                .ToListAsync();
            return participants;
        }

        public async Task<EventParticipantDto> RegisterToEventAsync(RegistrationDto registrationDto)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event evento = await _eventRepository.GetQueryable().Where(x => x.Id == registrationDto.EventId).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            // No puede registrarse a su propio evento
            if (evento.CreatedByUserId == user.Id)
                throw new Exception("No puedes registrarte a tu propio evento");

            // Evento pasado
            if (evento.EndDate < DateTime.UtcNow)
                throw new Exception("El evento ya finalizó");

            // Ya existe registro
            EventParticipant? existing = await _eventParticipantRepository.GetQueryable()
                .Where(x => x.UserId == user.Id && x.EventId == evento.Id).FirstOrDefaultAsync();

            if (existing != null)
                throw new Exception("Ya estás registrado en este evento");

            // Validar cupos (solo aprobados)
            int count = await _eventParticipantRepository.GetQueryable().Where(x => x.EventId == evento.Id && x.Status == ParticipantStatusEnums.Approved)
                .CountAsync();

            if (count >= evento.MaxParticipants)
                throw new Exception("El evento ya está lleno");

            EventParticipant response = await _eventParticipantRepository.CreateAsync(new()
            {
                EventId = evento.Id,
                UserId = user.Id,
                RegistrationDate = DateTime.UtcNow,
                Status = evento.IsPublic ? ParticipantStatusEnums.Approved : ParticipantStatusEnums.Pending,
                ConfirmationDate = evento.IsPublic ? DateTime.UtcNow : null
            });

            return new EventParticipantDto
            {
                Id = response.Id,
                UserId = response.UserId,
                UserName = response.User.UserName,
                UserFirstName = response.User.FirstName,
                UserLastName = response.User.LastName,
                EventId = response.EventId,
                Event = new EventResponseDto
                {
                    Id = response.Event.Id,
                    Name = response.Event.Name,
                    Description = response.Event.Description,
                    StartDate = response.Event.StartDate,
                    EndDate = response.Event.EndDate,
                    MaxParticipants = response.Event.MaxParticipants,
                    IsPublic = response.Event.IsPublic
                },
                RegistrationDate = response.RegistrationDate,
                Status = response.Status,
                ConfirmationDate = response.ConfirmationDate,
                CancellationReason = response.CancellationReason
            };
        }
    }
}
