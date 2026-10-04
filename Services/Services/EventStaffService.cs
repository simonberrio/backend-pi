using Dtos;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Services
{
    public class EventStaffService(IEventRepository eventRepository,
        IEventStaffRepository eventStaffRepository,
        IUserRepository userRepository,
        IUserService userService) : IEventStaffService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IEventStaffRepository _eventStaffRepository = eventStaffRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUserService _userService = userService;

        public async Task<EventStaffResponseDto> AddStaffAsync(EventStaffDto model)
        {
            Event @event = await GetEventOwnedByCurrentUserAsync(model.EventId);

            User targetUser = await _userRepository.GetByUserNameAsync(model.UserName) ??
                throw new Exception("No se encontró un usuario con ese nombre de usuario.");

            if (targetUser.Id == @event.CreatedByUserId)
                throw new Exception("El organizador ya tiene acceso total a este evento.");

            bool alreadyStaff = await _eventStaffRepository.GetQueryable()
                .AnyAsync(x => x.EventId == model.EventId && x.UserId == targetUser.Id);

            if (alreadyStaff)
                throw new Exception("Este usuario ya es staff de este evento.");

            EventStaff staff = await _eventStaffRepository.CreateAsync(new EventStaff
            {
                EventId = model.EventId,
                UserId = targetUser.Id,
                AssignedDate = DateTime.UtcNow
            });

            return new EventStaffResponseDto
            {
                Id = staff.Id,
                EventId = staff.EventId,
                UserId = targetUser.Id,
                UserName = targetUser.UserName,
                UserFirstName = targetUser.FirstName,
                UserLastName = targetUser.LastName,
                UserProfileImageUrl = targetUser.ProfileImageUrl,
                AssignedDate = staff.AssignedDate
            };
        }

        public async Task<List<EventStaffResponseDto>> GetStaffByEventIdAsync(int eventId)
        {
            await GetEventOwnedByCurrentUserAsync(eventId);

            return await _eventStaffRepository.GetQueryable()
                .Where(x => x.EventId == eventId)
                .Select(x => new EventStaffResponseDto
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    UserProfileImageUrl = x.User.ProfileImageUrl,
                    AssignedDate = x.AssignedDate
                })
                .ToListAsync();
        }

        public async Task RemoveStaffAsync(EventStaffDto model)
        {
            await GetEventOwnedByCurrentUserAsync(model.EventId);

            User targetUser = await _userRepository.GetByUserNameAsync(model.UserName) ??
                throw new Exception("No se encontró un usuario con ese nombre de usuario.");

            EventStaff staff = await _eventStaffRepository.GetQueryable()
                .Where(x => x.EventId == model.EventId && x.UserId == targetUser.Id).FirstOrDefaultAsync() ??
                throw new Exception("Este usuario no es staff de este evento.");

            await _eventStaffRepository.DeleteAsync(staff);
        }

        private async Task<Event> GetEventOwnedByCurrentUserAsync(int eventId)
        {
            User currentUser = await _userService.GetUserAuthenticatedAsync();

            Event @event = await _eventRepository.GetQueryable().Where(x => x.Id == eventId).FirstOrDefaultAsync() ??
                throw new Exception("Evento no encontrado.");

            if (@event.CreatedByUserId != currentUser.Id)
                throw new Exception("No tienes permisos para gestionar el staff de este evento.");

            return @event;
        }
    }
}
