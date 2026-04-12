using AutoMapper;
using Dtos;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class EventService(IEventRepository eventRepository, IMapper mapper, IUserService userService) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;

        public async Task<EventDto> CreateEventAsync(EventDto model)
        {
            var user = await _userService.GetUserAuthenticatedAsync();

            // Validaciones:
            if (model.StartDate >= model.EndDate)
                throw new Exception("La fecha de inicio debe ser menor a la fecha fin");

            if (model.MaxParticipants <= 0)
                throw new Exception("El cupo debe ser mayor a 0");

            Event entity = _mapper.Map<Event>(model);

            entity.CreatedByUserId = user.Id;
            entity.CreatedDate = DateTime.UtcNow;

            Event response = await _eventRepository.CreateAsync(entity);

            return _mapper.Map<EventDto>(response);
        }
    }
}
