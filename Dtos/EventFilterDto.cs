using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class EventFilterDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusInKm { get; set; } = 5; // default 5km

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public CategoryEnums? Category { get; set; }

        public string? Search { get; set; } // nombre
    }
}
