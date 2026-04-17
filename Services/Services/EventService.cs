using AutoMapper;
using Dtos;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;

namespace Services.Services
{
    public class EventService(IEventRepository eventRepository, IMapper mapper, IUserService userService) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;

        public async Task<EventResponseDto> CreateEventAsync(EventDto model)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            // Validaciones:
            if (model.StartDate >= model.EndDate)
                throw new Exception("La fecha de inicio debe ser menor a la fecha fin");

            if (model.MaxParticipants <= 0)
                throw new Exception("El cupo debe ser mayor a 0");

            Event entity = _mapper.Map<Event>(model);

            entity.CreatedByUserId = user.Id;
            entity.CreatedDate = DateTime.UtcNow;

            Event createdEvent = await _eventRepository.CreateAsync(entity);

            EventResponseDto response = _mapper.Map<EventResponseDto>(createdEvent);
            response.CreatedByUserName = $"{user.UserName}";

            return response;
        }

        private double DegreesToRadians(double deg)
        {
            return deg * (Math.PI / 180);
        }

        public async Task<EventResponseDto> DeleteEventAsync(int id)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event entity = await _eventRepository.GetQueryable().Where(x => x.Id == id).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            // Validar dueño
            if (entity.CreatedByUserId != user.Id)
                throw new Exception("No tienes permisos para eliminar este evento");

            Event response = await _eventRepository.DeleteAsync(entity);
            return _mapper.Map<EventResponseDto>(response);
        }

        private double GetDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) *
                Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public async Task<List<EventResponseDto>> GetEventsAsync(EventFilterDto filter)
        {
            IQueryable<Event> query = _eventRepository.GetQueryable();

            //// Solo eventos futuros
            //query = query.Where(e => e.EndDate >= DateTime.UtcNow);

            // Filtro por nombre
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(e => e.Name.ToLower().Contains(filter.Search.ToLower()));
            }

            // Filtro por categoría
            if (filter.Category.HasValue)
            {
                query = query.Where(e => e.Category == filter.Category.Value);
            }

            // Filtro por fecha
            if (filter.StartDate.HasValue)
            {
                query = query.Where(e => e.StartDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(e => e.EndDate <= filter.EndDate.Value);
            }

            // Filtro por distancia
            if (filter.Latitude.HasValue && filter.Longitude.HasValue)
            {
                var lat = filter.Latitude.Value;
                var lng = filter.Longitude.Value;
                var radius = filter.RadiusInKm ?? 5;

                query = query.Where(e =>
                    GetDistanceKm(lat, lng, e.Latitude, e.Longitude) <= radius
                );
            }

            List<EventResponseDto> responseDtos = await query.Select(x => new EventResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                Address = x.Address,
                MaxParticipants = x.MaxParticipants,
                IsPublic = x.IsPublic,
                Category = x.Category,
                Price = x.Price,
                CreatedByUserName = x.CreatedByUser.UserName
            }).ToListAsync();

            return responseDtos;
        }

        public async Task<EventResponseDto> UpdateEventAsync(EventDto model)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event entity = await _eventRepository.GetQueryable().Where(x => x.Id == model.Id).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            // Validar dueño
            if (entity.CreatedByUserId != user.Id)
                throw new Exception("No tienes permisos para editar este evento");

            // No editar eventos pasados
            if (entity.EndDate < DateTime.UtcNow)
                throw new Exception("No puedes editar eventos pasados");

            // Validaciones
            if (model.StartDate >= model.EndDate)
                throw new Exception("Fecha no válida");

            if (model.MaxParticipants <= 0)
                throw new Exception("Cupo inválido");

            // Mapear (sin perder campos importantes)
            _mapper.Map(model, entity);

            entity.StartDate = model.StartDate.ToUniversalTime();
            entity.EndDate = model.EndDate.ToUniversalTime();
            entity.UpdatedDate = DateTime.UtcNow;

            Event response = await _eventRepository.UpdateAsync(entity);

            return _mapper.Map<EventResponseDto>(response);
        }
    }
}
