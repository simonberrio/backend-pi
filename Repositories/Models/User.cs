using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [Required]
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
