using Dtos;
using Microsoft.EntityFrameworkCore;
using Repositories.IRepositories;
using Repositories.Models;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Services
{
    public class ReviewService(IEventRepository eventRepository,
        IEventParticipantRepository eventParticipantRepository,
        IReviewRepository reviewRepository,
        IUserService userService) : IReviewService
    {
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository = eventParticipantRepository;
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly IUserService _userService = userService;

        public async Task<ReviewResponseDto> CreateReview(ReviewDto model)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event @event = await _eventRepository.GetQueryable().Where(x => x.Id == model.EventId).FirstOrDefaultAsync()
                ?? throw new Exception("Evento no encontrado.");

            bool attended = await _eventParticipantRepository.GetQueryable()
                .AnyAsync(x => x.EventId == model.EventId && x.UserId == user.Id && x.Status == ParticipantStatusEnums.Approved);

            if (!attended)
                throw new Exception("Solo puedes reseñar eventos a los que estés inscrito.");

            bool alreadyReviewed = await _reviewRepository.GetQueryable()
                .AnyAsync(x => x.EventId == model.EventId && x.UserId == user.Id);

            if (alreadyReviewed)
                throw new Exception("Ya has reseñado este evento.");

            Review review = new()
            {
                EventId = model.EventId,
                UserId = user.Id,
                Rating = model.Rating,
                Comment = model.Comment,
                IsApproved = true,
                CreatedDate = DateTime.UtcNow
            };

            Review response = await _reviewRepository.CreateAsync(review);
            return await MapToResponseDto(response.Id);
        }

        public async Task DeleteReview(int eventId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Review review = await _reviewRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.UserId == user.Id).FirstOrDefaultAsync()
                ?? throw new Exception("No has reseñado este evento.");

            await _reviewRepository.DeleteAsync(review);
        }

        public async Task<List<ReviewResponseDto>> GetReviewsByEventId(int eventId)
        {
            return await _reviewRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.IsApproved)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new ReviewResponseDto
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    UserProfileImageUrl = x.User.ProfileImageUrl,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<ReviewSummaryDto> GetReviewSummaryByEventId(int eventId)
        {
            List<int> ratings = await _reviewRepository.GetQueryable()
                .Where(x => x.EventId == eventId && x.IsApproved)
                .Select(x => x.Rating)
                .ToListAsync();

            return new ReviewSummaryDto
            {
                TotalReviews = ratings.Count,
                AverageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 2)
            };
        }

        public async Task<ReviewResponseDto> UpdateReview(ReviewDto model)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Review review = await _reviewRepository.GetQueryable()
                .Where(x => x.EventId == model.EventId && x.UserId == user.Id).FirstOrDefaultAsync()
                ?? throw new Exception("No has reseñado este evento.");

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.UpdatedDate = DateTime.UtcNow;

            Review response = await _reviewRepository.UpdateAsync(review);
            return await MapToResponseDto(response.Id);
        }

        private async Task<ReviewResponseDto> MapToResponseDto(int reviewId)
        {
            return await _reviewRepository.GetQueryable()
                .Where(x => x.Id == reviewId)
                .Select(x => new ReviewResponseDto
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    UserProfileImageUrl = x.User.ProfileImageUrl,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .FirstAsync();
        }
    }
}
