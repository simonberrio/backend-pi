namespace Dtos
{
    // Nunca se devuelven los secretos, solo si están configurados
    public class PaymentSettingsResponseDto
    {
        public bool IsConfigured { get; set; }
        public string? PublicKey { get; set; }
        public string? Environment { get; set; } // "sandbox" o "production"
        public DateTime? UpdatedDate { get; set; }
    }
}
