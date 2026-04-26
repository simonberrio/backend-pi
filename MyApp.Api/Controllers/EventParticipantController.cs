using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace MyApp.Api.Controllers
{
    public class EventParticipantController(IEventParticipantService service) : ControllerBase
    {
        private readonly IEventParticipantService _service = service;

        [HttpPut("ApproveOrRejectParticipant")]
        [Authorize]
        public async Task<IActionResult> ApproveOrRejectParticipant(ManageParticipantDto manageParticipantDto)
        {
            EventParticipantDto result = await _service.ApproveOrRejectParticipant(manageParticipantDto);
            return Ok(result);
        }

        [HttpPut("CancelRegistration")]
        [Authorize]
        public async Task<IActionResult> CancelRegistrationAsync(RegistrationDto registrationDto)
        {
            EventParticipantDto result = await _service.CancelRegistrationAsync(registrationDto);
            return Ok(result);
        }

        [HttpGet("GetParticipantsByEventId")]
        [Authorize]
        public async Task<IActionResult> GetParticipantsByEventIdAsync([FromQuery] int eventId)
        {
            List<EventParticipantDto> result = await _service.GetParticipantsByEventIdAsync(eventId);
            return Ok(result);
        }

        [HttpGet("GetPendingRequestsAsync")]
        [Authorize]
        public async Task<IActionResult> GetPendingRequestsAsync([FromQuery] int eventId)
        {
            List<EventParticipantDto> result = await _service.GetPendingRequestsAsync(eventId);
            return Ok(result);
        }

        [HttpPost("RegisterToEvent")]
        [Authorize]
        public async Task<IActionResult> RegisterToEvent(RegistrationDto registrationDto)
        {
            EventParticipantDto result = await _service.RegisterToEventAsync(registrationDto);
            return Ok(result);
        }
    }
}
