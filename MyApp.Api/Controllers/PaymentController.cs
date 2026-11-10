using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController(IPaymentService service) : ControllerBase
    {
        private readonly IPaymentService _service = service;

        // El inscrito pide los datos para abrir el checkout de Wompi
        [HttpPost("Checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout([FromQuery] int eventId)
        {
            CheckoutResponseDto result = await _service.CreateCheckoutAsync(eventId);
            return Ok(result);
        }

        // transactionId es el "id" que Wompi agrega a la redirect-url al terminar el pago (opcional)
        [HttpGet("GetPaymentStatus")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus([FromQuery] string reference, [FromQuery] string? transactionId)
        {
            PaymentResponseDto result = await _service.GetPaymentStatusAsync(reference, transactionId);
            return Ok(result);
        }

        [HttpGet("GetPaymentsByEventId")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsByEventId([FromQuery] int eventId)
        {
            List<PaymentResponseDto> result = await _service.GetPaymentsByEventIdAsync(eventId);
            return Ok(result);
        }

        [HttpGet("GetMyPaymentSettings")]
        [Authorize]
        public async Task<IActionResult> GetMyPaymentSettings()
        {
            PaymentSettingsResponseDto result = await _service.GetMyPaymentSettingsAsync();
            return Ok(result);
        }

        [HttpPut("SavePaymentSettings")]
        [Authorize]
        public async Task<IActionResult> SavePaymentSettings(PaymentSettingsDto model)
        {
            PaymentSettingsResponseDto result = await _service.SavePaymentSettingsAsync(model);
            return Ok(result);
        }
    }
}
