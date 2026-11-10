namespace Repositories.Models
{
    // Intento de pago de una inscripción. Cada checkout genera una referencia única para Wompi.
    public class Payment
    {
        public int Id { get; set; }

        public string Reference { get; set; }

        public int EventParticipantId { get; set; }
        public EventParticipant EventParticipant { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }

        // Dueño de la cuenta Wompi que recibe el dinero (creador del evento)
        public string OrganizerUserId { get; set; }

        public long AmountInCents { get; set; }
        public string Currency { get; set; }

        public PaymentStatusEnums Status { get; set; }

        public string? WompiTransactionId { get; set; }
        public string? PaymentMethodType { get; set; }

        // Hasta cuándo el cupo queda reservado y el checkout de Wompi es válido
        public DateTime ExpiresAt { get; set; }

        // Último payload recibido de Wompi (útil para depurar)
        public string? LastWompiPayload { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
