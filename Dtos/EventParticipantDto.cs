using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dtos
{
    public class EventParticipantDto
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        public int EventId { get; set; }
        public EventResponseDto Event { get; set; }

        public DateTime RegistrationDate { get; set; }

        public ParticipantStatusEnums Status { get; set; }

        public DateTime? ConfirmationDate { get; set; }

        public string? CancellationReason { get; set; }
    }
}
