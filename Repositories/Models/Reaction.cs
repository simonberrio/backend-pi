using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class Reaction
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public ReactionTypeEnums Type { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
