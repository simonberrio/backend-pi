using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Services.Helpers
{
    public interface ISecretProtector
    {
        string Protect(string plainText);
        string Unprotect(string protectedText);
    }

    // Cifra los secretos de Wompi de los organizadores con AES-GCM.
    // La llave vive en configuración (Payments:SecretsEncryptionKey, 32 bytes en Base64), no en la BD,
    // así un volcado de la BD no expone los secretos.
    public class SecretProtector(IConfiguration config) : ISecretProtector
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly IConfiguration _config = config;

        private byte[] GetKey()
        {
            string? base64 = _config["Payments:SecretsEncryptionKey"];
            if (string.IsNullOrWhiteSpace(base64))
                throw new Exception("Falta configurar Payments:SecretsEncryptionKey en el servidor.");

            byte[] key = Convert.FromBase64String(base64);
            if (key.Length != 32)
                throw new Exception("Payments:SecretsEncryptionKey debe ser de 32 bytes en Base64.");
            return key;
        }

        public string Protect(string plainText)
        {
            byte[] plain = Encoding.UTF8.GetBytes(plainText);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] tag = new byte[TagSize];
            byte[] cipher = new byte[plain.Length];

            using AesGcm aes = new(GetKey(), TagSize);
            aes.Encrypt(nonce, plain, cipher, tag);

            // Formato: nonce | tag | cipher
            return Convert.ToBase64String([.. nonce, .. tag, .. cipher]);
        }

        public string Unprotect(string protectedText)
        {
            byte[] data = Convert.FromBase64String(protectedText);
            ReadOnlySpan<byte> span = data;
            ReadOnlySpan<byte> nonce = span[..NonceSize];
            ReadOnlySpan<byte> tag = span.Slice(NonceSize, TagSize);
            ReadOnlySpan<byte> cipher = span[(NonceSize + TagSize)..];
            byte[] plain = new byte[cipher.Length];

            using AesGcm aes = new(GetKey(), TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }
    }
}
