namespace Dtos
{
    // Llaves que el organizador copia de su dashboard de Wompi (Desarrolladores)
    public class PaymentSettingsDto
    {
        public string PublicKey { get; set; }
        public string IntegritySecret { get; set; }
        public string EventsSecret { get; set; }
    }
}
