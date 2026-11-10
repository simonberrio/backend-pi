using Dtos;

namespace Services.IService
{
    public enum WebhookResult
    {
        Processed,
        Ignored,
        InvalidSignature
    }

    public interface IPaymentService
    {
        Task<CheckoutResponseDto> CreateCheckoutAsync(int eventId);
        Task<PaymentSettingsResponseDto> GetMyPaymentSettingsAsync();
        Task<PaymentResponseDto> GetPaymentStatusAsync(string reference, string? transactionId);
        Task<List<PaymentResponseDto>> GetPaymentsByEventIdAsync(int eventId);
        Task<WebhookResult> HandleWompiEventAsync(string rawBody);
        Task<PaymentSettingsResponseDto> SavePaymentSettingsAsync(PaymentSettingsDto model);
    }
}
