using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.IService;
using System.Text.Json;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController(IPaymentService service, ILogger<WebhookController> logger) : ControllerBase
    {
        private readonly IPaymentService _service = service;
        private readonly ILogger<WebhookController> _logger = logger;

        // URL de eventos que cada organizador configura en su dashboard de Wompi.
        // No lleva JWT: la autenticidad se valida con el checksum firmado con el secreto de eventos.
        [HttpPost("Wompi")]
        [AllowAnonymous]
        public async Task<IActionResult> Wompi()
        {
            using StreamReader reader = new(Request.Body);
            string body = await reader.ReadToEndAsync();

            try
            {
                WebhookResult result = await _service.HandleWompiEventAsync(body);
                if (result == WebhookResult.InvalidSignature)
                    return Unauthorized();

                // Processed o Ignored: 200 para que Wompi no reintente
                return Ok();
            }
            catch (JsonException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                // 500 => Wompi reintentará el envío
                _logger.LogError(ex, "Error procesando webhook de Wompi");
                return StatusCode(500);
            }
        }
    }
}
