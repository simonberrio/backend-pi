using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.IService;
using Services.Services;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReactionController(IReactionService reactionService) : ControllerBase
    {
        private readonly IReactionService _reactionService = reactionService;

        [HttpDelete("Delete/{eventId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReaction(int eventId)
        {
            await _reactionService.DeleteReaction(eventId);
            return Ok();
        }

        [HttpGet("GetReactionsByEventId")]
        [Authorize]
        public async Task<IActionResult> GetReactionsByEventId(int eventId)
        {
            var result = await _reactionService.GetReactionsByEventId(eventId);
            return Ok(result);
        }

        [HttpPost("ReactToEvent")]
        [Authorize]
        public async Task<IActionResult> ReactToEvent(int eventId, ReactionTypeEnums reactionTypeId)
        {
            await _reactionService.ReactToEvent(eventId, reactionTypeId);
            return Ok();
        }
    }
}
