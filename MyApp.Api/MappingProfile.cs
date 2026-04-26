using AutoMapper;
using Dtos;
using Repositories.Models;

namespace MyApp.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EventDto, Event>();

            CreateMap<Event, EventResponseDto>();

            CreateMap<EventParticipant, EventParticipantDto>(); 

            CreateMap<User, UserResponseDto>();
        }
    }
}
