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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(EventDto model)
        {
            var result = await _eventService.CreateEventAsync(model);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _eventService.DeleteEventAsync(id);
            return Ok(new { message = "Evento eliminado correctamente" });
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] EventFilterDto filter)
        {
            var result = await _eventService.GetEventsAsync(filter);
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(EventDto model)
        {
            var result = await _eventService.UpdateEventAsync(model);
            return Ok(result);
        }
    }
}
