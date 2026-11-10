using Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.IRepositories;
using Repositories.Models;
using Services.Helpers;
using Services.IService;
using System.Net.NetworkInformation;

namespace Services.Services
{
    public class EventParticipantService(IEventRepository eventRepository,
        IEventParticipantRepository eventParticipantRepository,
        IEventStaffRepository eventStaffRepository,
        IPaymentRepository paymentRepository,
        IUserService userService) : IEventParticipantService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository = eventParticipantRepository;
        private readonly IEventStaffRepository _eventStaffRepository = eventStaffRepository;
        private readonly IUserService _userService = userService;

        public async Task<EventParticipantDto> ApproveOrRejectParticipant(ManageParticipantDto model)
        {
            User currentUser = await _userService.GetUserAuthenticatedAsync();

            EventParticipant participant = await _eventParticipantRepository.GetQueryable().Where(x => x.UserId == model.UserId && x.EventId == model.EventId)
                .Include(x => x.Event).FirstOrDefaultAsync()
                ?? throw new Exception("Error al buscar la persona en este evento.");

            //Solo creador puede gestionar
            if (participant.Event.CreatedByUserId != currentUser.Id)
                throw new Exception("No tienes permisos para gestionar este evento");

            //Solo eventos privados
            if (participant.Event.IsPublic)
                throw new Exception("Este evento no requiere aprobación");

            //Solo estado Pending
            if (participant.Status != ParticipantStatusEnums.Pending)
                throw new Exception("Esta solicitud ya no está pendiente");

            if (model.Approve)
            {
                //Validar cupos (aprobados + reservas de pago vigentes)
                int count = await _eventParticipantRepository.GetQueryable()
                    .CountOccupiedSpotsAsync(participant.EventId, participant.Id);

                if (count >= participant.Event.MaxParticipants)
                    throw new Exception("El evento ya está lleno");

                //Si el evento tiene precio, queda aprobado pero pendiente de pago
                await SetAdmittedStatusAsync(participant, participant.Event);
            }
            else
            {
                participant.Status = ParticipantStatusEnums.Rejected;
            }

            EventParticipant response = await _eventParticipantRepository.UpdateAsync(participant);
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

        public async Task<EventParticipantDto> CancelRegistrationAsync(RegistrationDto registrationDto)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            EventParticipant entity = await _eventParticipantRepository.GetQueryable().Where(x => x.UserId == user.Id &&
                x.EventId == registrationDto.EventId).FirstOrDefaultAsync()
                ?? throw new Exception("No estás registrado en este evento");

            if (entity.CheckedInDate != null)
                throw new Exception("No puedes cancelar tu inscripción porque ya hiciste check-in en este evento.");

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

        public async Task<List<EventParticipantDto>> GetParticipantsByEventIdAsync(int eventId)
        {
            List<EventParticipantDto> participants = await _eventParticipantRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.Status == ParticipantStatusEnums.Approved)
                .Select(x => new EventParticipantDto
                {
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

        public async Task<List<EventParticipantDto>> GetPendingRequestsAsync(int eventId)
        {
            User currentUser = await _userService.GetUserAuthenticatedAsync();

            Event evento = await _eventRepository.GetQueryable().Where(x => x.Id == eventId).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            if (evento.CreatedByUserId != currentUser.Id)
                throw new Exception("No tienes permisos para gestionar este evento");

            return await _eventParticipantRepository.GetQueryable().Where(x => x.EventId == eventId && x.Status == ParticipantStatusEnums.Pending)
                .Select(x => new EventParticipantDto
                {
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
                }).ToListAsync();
        }

        public async Task<EventParticipantDto> RegisterToEventAsync(RegistrationDto registrationDto)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event evento = await _eventRepository.GetQueryable().Where(x => x.Id == registrationDto.EventId).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            //No puede registrarse a su propio evento
            if (evento.CreatedByUserId == user.Id)
                throw new Exception("No puedes registrarte a tu propio evento");

            //Evento pasado
            if (evento.EndDate < DateTime.UtcNow)
                throw new Exception("El evento ya finalizó");

            EventParticipant response = new();

            //Ya existe registro
            EventParticipant? eventParticipant = await _eventParticipantRepository.GetQueryable()
                .Where(x => x.UserId == user.Id && x.EventId == evento.Id).FirstOrDefaultAsync();
            if (eventParticipant == null)
            {
                //Validar cupos (aprobados + reservas de pago vigentes)
                int count = await _eventParticipantRepository.GetQueryable().CountOccupiedSpotsAsync(evento.Id);

                if (count >= evento.MaxParticipants)
                    throw new Exception("El evento ya está lleno");

                EventParticipant newParticipant = new()
                {
                    EventId = evento.Id,
                    UserId = user.Id,
                    RegistrationDate = DateTime.UtcNow,
                    Status = ParticipantStatusEnums.Pending
                };

                //Público: entra directo (o queda pendiente de pago). Privado: espera aprobación.
                if (evento.IsPublic)
                    await SetAdmittedStatusAsync(newParticipant, evento);

                response = await _eventParticipantRepository.CreateAsync(newParticipant);
            }
            else
            {
                if (eventParticipant.Status == ParticipantStatusEnums.Cancelled)
                {
                    if (evento.IsPublic)
                    {
                        int count = await _eventParticipantRepository.GetQueryable()
                            .CountOccupiedSpotsAsync(evento.Id, eventParticipant.Id);

                        if (count >= evento.MaxParticipants)
                            throw new Exception("El evento ya está lleno");
                    }

                    eventParticipant.RegistrationDate = DateTime.UtcNow;
                    eventParticipant.CancellationReason = null;
                    eventParticipant.Status = ParticipantStatusEnums.Pending;
                    eventParticipant.ConfirmationDate = null;

                    if (evento.IsPublic)
                        await SetAdmittedStatusAsync(eventParticipant, evento);

                    response = await _eventParticipantRepository.UpdateAsync(eventParticipant);
                }
                else if (eventParticipant.Status == ParticipantStatusEnums.AwaitingPayment)
                {
                    //Ya inscrito, solo le falta pagar: se devuelve la inscripción para que el front abra el checkout
                    response = await _eventParticipantRepository.GetQueryable()
                        .Include(x => x.User).Include(x => x.Event)
                        .FirstAsync(x => x.Id == eventParticipant.Id);
                }
                else if (eventParticipant.Status == ParticipantStatusEnums.Pending)
                {
                    throw new ArgumentException("Tu solicitud para este evento aún está pendiente de aprobación");
                }
                else if (eventParticipant.Status == ParticipantStatusEnums.Approved)
                {
                    throw new Exception("Ya estás registrado en este evento");
                }
                else if (eventParticipant.Status == ParticipantStatusEnums.Rejected)
                {
                    throw new Exception("Tu solicitud para este evento fue rechazada");
                }
            }

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

        public async Task<CheckInCodeResponseDto> GetMyCheckInCodeAsync(int eventId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            EventParticipant participant = await _eventParticipantRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.UserId == user.Id).FirstOrDefaultAsync()
                ?? throw new Exception("No estás inscrito en este evento.");

            if (participant.Status == ParticipantStatusEnums.AwaitingPayment)
                throw new Exception("Debes completar el pago para obtener tu código de check-in.");

            if (participant.Status != ParticipantStatusEnums.Approved)
                throw new Exception("Tu inscripción a este evento aún no está aprobada.");

            if (participant.CheckInCode == null)
                throw new Exception("Aún no se ha generado tu código de check-in.");

            return new CheckInCodeResponseDto
            {
                EventId = eventId,
                CheckInCode = participant.CheckInCode.Value
            };
        }

        // Deja la inscripción admitida: Approved si es gratis (o si ya tiene un pago aprobado previo),
        // AwaitingPayment si el evento tiene precio y aún no ha pagado.
        private async Task SetAdmittedStatusAsync(EventParticipant participant, Event evento)
        {
            bool alreadyPaid = participant.Id != 0 && await _paymentRepository.GetQueryable()
                .AnyAsync(x => x.EventParticipantId == participant.Id && x.Status == PaymentStatusEnums.Approved);

            if (evento.RequiresPayment() && !alreadyPaid)
            {
                participant.Status = ParticipantStatusEnums.AwaitingPayment;
                participant.ConfirmationDate = null;
                return;
            }

            participant.Status = ParticipantStatusEnums.Approved;
            participant.ConfirmationDate = DateTime.UtcNow;
            participant.CheckInCode ??= Guid.NewGuid();
        }

        public async Task<EventParticipantDto> CheckInAsync(CheckInDto model)
        {
            User currentUser = await _userService.GetUserAuthenticatedAsync();

            EventParticipant participant = await _eventParticipantRepository.GetQueryable()
                .Include(x => x.Event)
                .Include(x => x.User)
                .Where(x => x.CheckInCode == model.CheckInCode)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Código de check-in inválido.");

            bool isOrganizer = participant.Event.CreatedByUserId == currentUser.Id;
            bool isStaff = await _eventStaffRepository.GetQueryable()
                .AnyAsync(x => x.EventId == participant.EventId && x.UserId == currentUser.Id);

            if (!isOrganizer && !isStaff)
                throw new Exception("No tienes permisos para hacer check-in en este evento.");

            if (participant.Status != ParticipantStatusEnums.Approved)
                throw new Exception("Esta inscripción no está aprobada.");

            if (participant.CheckedInDate != null)
                throw new Exception("Ya se registró la asistencia para esta inscripción.");

            participant.CheckedInDate = DateTime.UtcNow;

            EventParticipant response = await _eventParticipantRepository.UpdateAsync(participant);
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
