using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController(IEventService eventService) : ControllerBase
    {
        private readonly IEventService _eventService = eventService;

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create(EventDto model)
        {
            EventResponseDto result = await _eventService.CreateEventAsync(model);
            return Ok(result);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            EventResponseDto result = await _eventService.DeleteEventAsync(id);
            return Ok(result);
        }

        [HttpGet("GetEvents")]
        public async Task<IActionResult> GetEvents([FromQuery] EventFilterDto filter)
        {
            List<EventResponseDto> result = await _eventService.GetEventsAsync(filter);
            return Ok(result);
        }

        [HttpPut("Update")]
        [Authorize]
        public async Task<IActionResult> Update(EventDto model)
        {
            EventResponseDto result = await _eventService.UpdateEventAsync(model);
            return Ok(result);
        }
    }
}
