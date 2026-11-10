using Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.IRepositories;
using Repositories.Models;
using Services.Helpers;
using Services.IService;
using System.Text.Json;

namespace Services.Services
{
    public class PaymentService(IPaymentRepository paymentRepository,
        IOrganizerPaymentSettingsRepository settingsRepository,
        IEventParticipantRepository eventParticipantRepository,
        IEventRepository eventRepository,
        IUserService userService,
        ISecretProtector secretProtector,
        HttpClient httpClient,
        ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IOrganizerPaymentSettingsRepository _settingsRepository = settingsRepository;
        private readonly IEventParticipantRepository _eventParticipantRepository = eventParticipantRepository;
        private readonly IEventRepository _eventRepository = eventRepository;
        private readonly IUserService _userService = userService;
        private readonly ISecretProtector _secretProtector = secretProtector;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<PaymentService> _logger = logger;

        #region Configuración del organizador

        public async Task<PaymentSettingsResponseDto> SavePaymentSettingsAsync(PaymentSettingsDto model)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            string publicKey = model.PublicKey?.Trim() ?? "";
            string integritySecret = model.IntegritySecret?.Trim() ?? "";
            string eventsSecret = model.EventsSecret?.Trim() ?? "";

            // Las llaves de Wompi tienen prefijos según el ambiente; todas deben ser del mismo
            bool isTest = publicKey.StartsWith("pub_test_");
            bool isProd = publicKey.StartsWith("pub_prod_");
            if (!isTest && !isProd)
                throw new Exception("La llave pública no es válida (debe empezar por pub_test_ o pub_prod_).");

            string env = isTest ? "test" : "prod";
            if (!integritySecret.StartsWith($"{env}_integrity_"))
                throw new Exception($"El secreto de integridad no es válido (debe empezar por {env}_integrity_).");
            if (!eventsSecret.StartsWith($"{env}_events_"))
                throw new Exception($"El secreto de eventos no es válido (debe empezar por {env}_events_).");

            OrganizerPaymentSettings? settings = await _settingsRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            if (settings == null)
            {
                settings = await _settingsRepository.CreateAsync(new OrganizerPaymentSettings
                {
                    UserId = user.Id,
                    PublicKey = publicKey,
                    IntegritySecretEncrypted = _secretProtector.Protect(integritySecret),
                    EventsSecretEncrypted = _secretProtector.Protect(eventsSecret),
                    CreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                settings.PublicKey = publicKey;
                settings.IntegritySecretEncrypted = _secretProtector.Protect(integritySecret);
                settings.EventsSecretEncrypted = _secretProtector.Protect(eventsSecret);
                settings.UpdatedDate = DateTime.UtcNow;
                settings = await _settingsRepository.UpdateAsync(settings);
            }

            return MapSettings(settings);
        }

        public async Task<PaymentSettingsResponseDto> GetMyPaymentSettingsAsync()
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            OrganizerPaymentSettings? settings = await _settingsRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == user.Id);

            return MapSettings(settings);
        }

        private static PaymentSettingsResponseDto MapSettings(OrganizerPaymentSettings? settings)
        {
            if (settings == null)
                return new PaymentSettingsResponseDto { IsConfigured = false };

            return new PaymentSettingsResponseDto
            {
                IsConfigured = true,
                PublicKey = settings.PublicKey,
                Environment = WompiHelper.GetEnvironment(settings.PublicKey),
                UpdatedDate = settings.UpdatedDate ?? settings.CreatedDate
            };
        }

        #endregion

        #region Checkout

        public async Task<CheckoutResponseDto> CreateCheckoutAsync(int eventId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            EventParticipant participant = await _eventParticipantRepository.GetQueryable()
                .Include(x => x.Event)
                .Where(x => x.EventId == eventId && x.UserId == user.Id)
                .FirstOrDefaultAsync()
                ?? throw new Exception("No estás inscrito en este evento.");

            Event evento = participant.Event;

            if (!evento.RequiresPayment())
                throw new Exception("Este evento no requiere pago.");

            if (evento.EndDate < DateTime.UtcNow)
                throw new Exception("El evento ya finalizó.");

            switch (participant.Status)
            {
                case ParticipantStatusEnums.Approved:
                    throw new Exception("Tu inscripción ya está pagada y aprobada.");
                case ParticipantStatusEnums.Pending:
                    throw new Exception("Tu solicitud aún debe ser aprobada por el organizador antes de pagar.");
                case ParticipantStatusEnums.Rejected:
                    throw new Exception("Tu solicitud para este evento fue rechazada.");
                case ParticipantStatusEnums.Cancelled:
                    throw new Exception("Cancelaste tu inscripción. Inscríbete de nuevo para pagar.");
            }

            OrganizerPaymentSettings settings = await _settingsRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == evento.CreatedByUserId)
                ?? throw new Exception("El organizador aún no ha configurado su cuenta de pagos.");

            string integritySecret = _secretProtector.Unprotect(settings.IntegritySecretEncrypted);
            long amountInCents = (long)Math.Round(evento.Price!.Value * 100m, MidpointRounding.AwayFromZero);

            // Reutilizar un intento vigente (mismo monto) para no crear referencias de más
            Payment? active = await _paymentRepository.GetQueryable()
                .Where(x => x.EventParticipantId == participant.Id &&
                            x.Status == PaymentStatusEnums.Pending &&
                            x.AmountInCents == amountInCents &&
                            x.ExpiresAt > DateTime.UtcNow.AddMinutes(2))
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            if (active == null)
            {
                int occupied = await _eventParticipantRepository.GetQueryable()
                    .CountOccupiedSpotsAsync(evento.Id, participant.Id);

                if (occupied >= evento.MaxParticipants)
                    throw new Exception("El evento ya está lleno.");

                active = await _paymentRepository.CreateAsync(new Payment
                {
                    Reference = $"EV{evento.Id}-P{participant.Id}-{Guid.NewGuid():N}"[..30],
                    EventParticipantId = participant.Id,
                    EventId = evento.Id,
                    OrganizerUserId = evento.CreatedByUserId!,
                    AmountInCents = amountInCents,
                    Currency = WompiHelper.Currency,
                    Status = PaymentStatusEnums.Pending,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(CapacityExtensions.ReservationMinutes),
                    CreatedDate = DateTime.UtcNow
                });
            }

            string expiration = WompiHelper.FormatExpiration(active.ExpiresAt);

            return new CheckoutResponseDto
            {
                PublicKey = settings.PublicKey,
                Reference = active.Reference,
                AmountInCents = active.AmountInCents,
                Currency = active.Currency,
                ExpirationTime = expiration,
                IntegritySignature = WompiHelper.BuildIntegritySignature(active.Reference, active.AmountInCents,
                    active.Currency, expiration, integritySecret)
            };
        }

        #endregion

        #region Consultas

        public async Task<PaymentResponseDto> GetPaymentStatusAsync(string reference, string? transactionId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Payment payment = await _paymentRepository.GetQueryable()
                .Include(x => x.EventParticipant).ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Reference == reference)
                ?? throw new Exception("Pago no encontrado.");

            if (payment.EventParticipant.UserId != user.Id && payment.OrganizerUserId != user.Id)
                throw new Exception("No tienes permisos para ver este pago.");

            // Respaldo del webhook: si el front vuelve del checkout con el id de la transacción,
            // se consulta directamente a Wompi (fuente confiable) y se aplica el resultado.
            if (payment.Status == PaymentStatusEnums.Pending && !string.IsNullOrWhiteSpace(transactionId))
                await SyncFromWompiAsync(payment, transactionId);

            return MapPayment(payment);
        }

        public async Task<List<PaymentResponseDto>> GetPaymentsByEventIdAsync(int eventId)
        {
            User user = await _userService.GetUserAuthenticatedAsync();

            Event evento = await _eventRepository.GetQueryable().FirstOrDefaultAsync(x => x.Id == eventId)
                ?? throw new Exception("Evento no encontrado.");

            if (evento.CreatedByUserId != user.Id)
                throw new Exception("No tienes permisos para ver los pagos de este evento.");

            List<Payment> payments = await _paymentRepository.GetQueryable()
                .Include(x => x.EventParticipant).ThenInclude(x => x.User)
                .Where(x => x.EventId == eventId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return payments.Select(MapPayment).ToList();
        }

        private static PaymentResponseDto MapPayment(Payment x) => new()
        {
            Id = x.Id,
            Reference = x.Reference,
            EventId = x.EventId,
            EventParticipantId = x.EventParticipantId,
            UserId = x.EventParticipant?.UserId,
            UserName = x.EventParticipant?.User?.UserName,
            AmountInCents = x.AmountInCents,
            Currency = x.Currency,
            Status = x.Status,
            ParticipantStatus = x.EventParticipant?.Status ?? default,
            WompiTransactionId = x.WompiTransactionId,
            PaymentMethodType = x.PaymentMethodType,
            ExpiresAt = x.ExpiresAt,
            CreatedDate = x.CreatedDate,
            UpdatedDate = x.UpdatedDate
        };

        private async Task SyncFromWompiAsync(Payment payment, string transactionId)
        {
            OrganizerPaymentSettings? settings = await _settingsRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == payment.OrganizerUserId);
            if (settings == null)
                return;

            try
            {
                string url = $"{WompiHelper.GetApiBaseUrl(settings.PublicKey)}/transactions/{Uri.EscapeDataString(transactionId)}";
                using HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Wompi respondió {Status} al consultar la transacción {TransactionId}", response.StatusCode, transactionId);
                    return;
                }

                string body = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement transaction = doc.RootElement.GetProperty("data");

                await ApplyTransactionAsync(payment, transaction, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando la transacción {TransactionId} en Wompi", transactionId);
            }
        }

        #endregion

        #region Webhook

        public async Task<WebhookResult> HandleWompiEventAsync(string rawBody)
        {
            using JsonDocument doc = JsonDocument.Parse(rawBody);
            JsonElement root = doc.RootElement;

            string? eventType = root.TryGetProperty("event", out JsonElement ev) ? ev.GetString() : null;
            if (eventType != "transaction.updated" ||
                !root.TryGetProperty("data", out JsonElement data) ||
                !data.TryGetProperty("transaction", out JsonElement transaction))
            {
                _logger.LogInformation("Evento de Wompi ignorado: {Event}", eventType);
                return WebhookResult.Ignored;
            }

            string? reference = transaction.TryGetProperty("reference", out JsonElement refEl) ? refEl.GetString() : null;

            // La referencia aún no es confiable: solo se usa para saber con qué secreto validar
            Payment? payment = await _paymentRepository.GetQueryable()
                .Include(x => x.EventParticipant)
                .FirstOrDefaultAsync(x => x.Reference == reference);
            if (payment == null)
            {
                _logger.LogWarning("Webhook de Wompi con referencia desconocida: {Reference}", reference);
                return WebhookResult.Ignored;
            }

            OrganizerPaymentSettings? settings = await _settingsRepository.GetQueryable()
                .FirstOrDefaultAsync(x => x.UserId == payment.OrganizerUserId);
            if (settings == null)
            {
                _logger.LogWarning("El organizador del pago {Reference} no tiene llaves configuradas", reference);
                return WebhookResult.Ignored;
            }

            string eventsSecret = _secretProtector.Unprotect(settings.EventsSecretEncrypted);
            if (!WompiHelper.IsValidEventSignature(root, eventsSecret))
            {
                _logger.LogWarning("Firma inválida en webhook de Wompi para la referencia {Reference}", reference);
                return WebhookResult.InvalidSignature;
            }

            await ApplyTransactionAsync(payment, transaction, rawBody);
            return WebhookResult.Processed;
        }

        // Aplica el estado de una transacción de Wompi (ya verificada) al pago y la inscripción. Es idempotente.
        private async Task ApplyTransactionAsync(Payment payment, JsonElement transaction, string rawPayload)
        {
            string? reference = transaction.GetProperty("reference").GetString();
            long amount = transaction.GetProperty("amount_in_cents").GetInt64();
            string? currency = transaction.GetProperty("currency").GetString();
            string? status = transaction.GetProperty("status").GetString();

            if (reference != payment.Reference)
            {
                _logger.LogWarning("La transacción no corresponde a la referencia {Reference}", payment.Reference);
                return;
            }

            // Un pago ya aprobado no se vuelve a tocar (reintentos del webhook)
            if (payment.Status == PaymentStatusEnums.Approved)
                return;

            PaymentStatusEnums newStatus = WompiHelper.MapStatus(status);

            if (amount != payment.AmountInCents || currency != payment.Currency)
            {
                _logger.LogError("Monto/moneda no coinciden para {Reference}: esperado {Expected} {Cur}, recibido {Amount} {RCur}",
                    payment.Reference, payment.AmountInCents, payment.Currency, amount, currency);
                newStatus = PaymentStatusEnums.Error;
            }

            payment.Status = newStatus;
            payment.WompiTransactionId = transaction.TryGetProperty("id", out JsonElement id) ? id.GetString() : payment.WompiTransactionId;
            payment.PaymentMethodType = transaction.TryGetProperty("payment_method_type", out JsonElement pm) ? pm.GetString() : payment.PaymentMethodType;
            payment.LastWompiPayload = rawPayload;
            payment.UpdatedDate = DateTime.UtcNow;
            await _paymentRepository.UpdateAsync(payment);

            if (newStatus != PaymentStatusEnums.Approved)
                return;

            EventParticipant participant = payment.EventParticipant;
            if (participant.Status == ParticipantStatusEnums.AwaitingPayment)
            {
                participant.Status = ParticipantStatusEnums.Approved;
                participant.ConfirmationDate = DateTime.UtcNow;
                participant.CheckInCode ??= Guid.NewGuid();
                await _eventParticipantRepository.UpdateAsync(participant);
            }
            else
            {
                // Ej.: canceló mientras pagaba. Queda registrado para que el organizador reembolse manualmente.
                _logger.LogWarning("Pago aprobado {Reference} para una inscripción en estado {Status}", payment.Reference, participant.Status);
            }
        }

        #endregion
    }
}
