using Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.IService
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateReview(ReviewDto model);
        Task DeleteReview(int eventId);
        Task<List<ReviewResponseDto>> GetReviewsByEventId(int eventId);
        Task<ReviewSummaryDto> GetReviewSummaryByEventId(int eventId);
        Task<ReviewResponseDto> UpdateReview(ReviewDto model);
    }
}
