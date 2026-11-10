using Repositories.Models;

namespace Dtos
{
    public class PaymentResponseDto
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public int EventId { get; set; }
        public int EventParticipantId { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public long AmountInCents { get; set; }
        public string Currency { get; set; }
        public PaymentStatusEnums Status { get; set; }
        public ParticipantStatusEnums ParticipantStatus { get; set; }
        public string? WompiTransactionId { get; set; }
        public string? PaymentMethodType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
