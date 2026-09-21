using System.ComponentModel.DataAnnotations;

namespace Dtos
{
    public class ReviewDto
    {
        public int EventId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }
    }
}
