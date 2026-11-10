namespace Dtos
{
    // Datos que el front necesita para abrir el Widget / Web Checkout de Wompi
    public class CheckoutResponseDto
    {
        public string PublicKey { get; set; }
        public string Reference { get; set; }
        public long AmountInCents { get; set; }
        public string Currency { get; set; }
        public string IntegritySignature { get; set; }
        public string ExpirationTime { get; set; } // ISO 8601 UTC, tal como se usó en la firma
    }
}
