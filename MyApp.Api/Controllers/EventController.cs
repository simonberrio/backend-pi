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
    }
}
