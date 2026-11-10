using Repositories.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Services.Helpers
{
    public static class WompiHelper
    {
        public const string Currency = "COP";

        public static bool IsSandboxKey(string publicKey) => publicKey.StartsWith("pub_test_");

        public static string GetEnvironment(string publicKey) => IsSandboxKey(publicKey) ? "sandbox" : "production";

        public static string GetApiBaseUrl(string publicKey) =>
            IsSandboxKey(publicKey) ? "https://sandbox.wompi.co/v1" : "https://production.wompi.co/v1";

        // Formato que espera Wompi para expiration-time, p. ej. 2026-11-03T20:28:50.000Z
        public static string FormatExpiration(DateTime utc) =>
            utc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Firma de integridad: SHA256(reference + amountInCents + currency + expirationTime + integritySecret)
        public static string BuildIntegritySignature(string reference, long amountInCents, string currency,
            string expirationTime, string integritySecret)
        {
            return Sha256Hex($"{reference}{amountInCents}{currency}{expirationTime}{integritySecret}");
        }

        // Valida el checksum de un evento de Wompi:
        // SHA256(valores de signature.properties (en orden) + timestamp + eventsSecret) == signature.checksum
        public static bool IsValidEventSignature(JsonElement root, string eventsSecret)
        {
            if (!root.TryGetProperty("signature", out JsonElement signature) ||
                !signature.TryGetProperty("properties", out JsonElement properties) ||
                !signature.TryGetProperty("checksum", out JsonElement checksumElement) ||
                !root.TryGetProperty("timestamp", out JsonElement timestamp) ||
                !root.TryGetProperty("data", out JsonElement data))
                return false;

            StringBuilder sb = new();
            foreach (JsonElement property in properties.EnumerateArray())
            {
                JsonElement current = data;
                foreach (string part in (property.GetString() ?? "").Split('.'))
                {
                    if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                        return false;
                }
                sb.Append(JsonValueToString(current));
            }
            sb.Append(JsonValueToString(timestamp));
            sb.Append(eventsSecret);

            string expected = Sha256Hex(sb.ToString());
            string received = (checksumElement.GetString() ?? "").ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(received));
        }

        public static PaymentStatusEnums MapStatus(string? wompiStatus) => wompiStatus switch
        {
            "APPROVED" => PaymentStatusEnums.Approved,
            "DECLINED" => PaymentStatusEnums.Declined,
            "VOIDED" => PaymentStatusEnums.Voided,
            "ERROR" => PaymentStatusEnums.Error,
            _ => PaymentStatusEnums.Pending
        };

        private static string JsonValueToString(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Null => "",
            _ => element.GetRawText()
        };

        private static string Sha256Hex(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }
}
