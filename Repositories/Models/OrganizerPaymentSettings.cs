namespace Repositories.Models
{
    // Llaves de la cuenta Wompi del organizador. Los secretos se guardan cifrados.
    public class OrganizerPaymentSettings
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string PublicKey { get; set; }
        public string IntegritySecretEncrypted { get; set; }
        public string EventsSecretEncrypted { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
