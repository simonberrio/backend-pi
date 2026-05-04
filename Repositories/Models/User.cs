using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repositories.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string LastName { get; set; }

        public string? ProfileImageUrl { get; set; }
        public string? ProfileImagePublicId { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
