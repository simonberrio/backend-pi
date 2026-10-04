using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventStaffController(IEventStaffService eventStaffService) : ControllerBase
    {
        private readonly IEventStaffService _eventStaffService = eventStaffService;

        [HttpPost("AddStaff")]
        [Authorize]
        public async Task<IActionResult> AddStaff(EventStaffDto model)
        {
            var result = await _eventStaffService.AddStaffAsync(model);
            return Ok(result);
        }

        [HttpGet("GetStaffByEventId")]
        [Authorize]
        public async Task<IActionResult> GetStaffByEventId([FromQuery] int eventId)
        {
            var result = await _eventStaffService.GetStaffByEventIdAsync(eventId);
            return Ok(result);
        }

        [HttpDelete("RemoveStaff")]
        [Authorize]
        public async Task<IActionResult> RemoveStaff([FromQuery] EventStaffDto model)
        {
            await _eventStaffService.RemoveStaffAsync(model);
            return Ok();
        }
    }
}
