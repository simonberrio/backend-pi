namespace Repositories.Models
{
    public class EventParticipant
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime RegistrationDate { get; set; }

        public ParticipantStatusEnums Status { get; set; }

        public DateTime? ConfirmationDate { get; set; }
        public DateTime? CheckInDate { get; set; }

        public string? CancellationReason { get; set; }
    }
}
