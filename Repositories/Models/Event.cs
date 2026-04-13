using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }

        public int MaxParticipants { get; set; }
        public bool IsPublic { get; set; }

        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }

        public CategoryEnums Category { get; set; }

        public string CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public ICollection<EventParticipant> Participants { get; set; }
    }
}
