using System;

namespace Repositories.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
