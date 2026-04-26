using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class ManageParticipantDto
    {
        public int EventId { get; set; }
        public string UserId { get; set; }
        public bool Approve { get; set; }
    }
}
