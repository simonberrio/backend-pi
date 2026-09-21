using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewController(IReviewService reviewService) : ControllerBase
    {
        private readonly IReviewService _reviewService = reviewService;

        [HttpPost("Create")]
        [Authorize]
        public async Task<IActionResult> Create(ReviewDto model)
        {
            var result = await _reviewService.CreateReview(model);
            return Ok(result);
        }

        [HttpPut("Update")]
        [Authorize]
        public async Task<IActionResult> Update(ReviewDto model)
        {
            var result = await _reviewService.UpdateReview(model);
            return Ok(result);
        }

        [HttpDelete("Delete/{eventId}")]
        [Authorize]
        public async Task<IActionResult> Delete(int eventId)
        {
            await _reviewService.DeleteReview(eventId);
            return Ok();
        }

        [HttpGet("GetReviewsByEventId")]
        public async Task<IActionResult> GetReviewsByEventId(int eventId)
        {
            var result = await _reviewService.GetReviewsByEventId(eventId);
            return Ok(result);
        }

        [HttpGet("GetReviewSummaryByEventId")]
        public async Task<IActionResult> GetReviewSummaryByEventId(int eventId)
        {
            var result = await _reviewService.GetReviewSummaryByEventId(eventId);
            return Ok(result);
        }
    }
}
