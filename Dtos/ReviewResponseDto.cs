using System;

namespace Dtos
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string? UserProfileImageUrl { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
