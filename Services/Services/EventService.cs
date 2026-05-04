using AutoMapper;
using Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;

namespace Services.Services
{
    public class EventService(IEventRepository eventRepository,
        IEventParticipantRepository eventParticipantRepository,
        IImageService imageService,
        IMapper mapper,
        IUserService userService) : IEventService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository = eventParticipantRepository;
        private readonly IImageService _imageService = imageService;
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
            entity.CreatedByUserName = user.UserName;
            entity.CreatedDate = DateTime.UtcNow;

            Event createdEvent = await _eventRepository.CreateAsync(entity);

            EventResponseDto response = _mapper.Map<EventResponseDto>(createdEvent);

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
                query = query.Where(x => x.Name.ToLower().Contains(filter.Search.ToLower()));
            }

            // Filtro por categoría
            if (filter.Category.HasValue)
            {
                query = query.Where(x => x.Category == filter.Category.Value);
            }

            // Filtro por fecha
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.StartDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => x.EndDate <= filter.EndDate.Value);
            }

            // Filtro por distancia
            if (filter.Latitude.HasValue && filter.Longitude.HasValue)
            {
                var lat = filter.Latitude.Value;
                var lng = filter.Longitude.Value;
                var radius = filter.RadiusInKm ?? 5;

                query = query.Where(x =>
                    GetDistanceKm(lat, lng, x.Latitude, x.Longitude) <= radius
                );
            }

            List<EventResponseDto> responseDtos = _mapper.Map<List<EventResponseDto>>(await query.Include(x => x.CreatedByUser).ToListAsync());

            return responseDtos;
        }

        public async Task<List<EventResponseDto>> GetEventsIAmRegistered()
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            List<EventParticipant> participants = await _eventParticipantRepository.GetQueryable().Where(x => x.UserId == user.Id).Include(x => x.Event)
                .ThenInclude(x => x.CreatedByUser).ToListAsync();

            return _mapper.Map<List<EventResponseDto>>(participants.Select(x => x.Event).ToList());
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

        public async Task<EventResponseDto> UploadImageAsync(int eventId, IFormFile formFile)
        {
            User user = await _userService.GetUserAuthenticatedAsync();
            Event entity = await _eventRepository.GetQueryable().Where(x => x.Id == eventId).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado");

            if (entity.CreatedByUserId != user.Id)
                throw new Exception("No tienes permisos para editar este evento");

            if (entity.EndDate < DateTime.UtcNow)
                throw new Exception("No puedes editar eventos pasados");

            if (entity.ImagePublicId != null)
                await _imageService.DeleteImageAsync(entity.ImagePublicId);

            ImageResultDto imageResult = await _imageService.UploadImageAsync(formFile);
            entity.ImageUrl = imageResult.Url;
            entity.ImagePublicId = imageResult.PublicId;
            entity.UpdatedDate = DateTime.UtcNow;
            Event response = await _eventRepository.UpdateAsync(entity);
            return _mapper.Map<EventResponseDto>(response);
        }
    }
}
