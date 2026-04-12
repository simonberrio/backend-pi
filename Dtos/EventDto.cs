using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class EventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string Address { get; set; }

        public int MaxParticipants { get; set; }
        public bool IsPublic { get; set; }

        public CategoryEnums Category { get; set; }

        public decimal? Price { get; set; } 

        // Imagen por ahora como string (luego hacemos upload real)
        public string? ImageUrl { get; set; }

        public string CreatedByUserName { get; set; }
    }
}
