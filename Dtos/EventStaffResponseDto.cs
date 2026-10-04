using System;

namespace Dtos
{
    public class EventStaffResponseDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string? UserProfileImageUrl { get; set; }

        public DateTime AssignedDate { get; set; }
    }
}
