using OtpNet;
using System.Security.Cryptography;

namespace SocialMediaBackend.Services
{
    public class MfaService
    {
        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GenerateQrCodeUri(string email, string secretKey, string issuer = "YourApp")
        {
            return $"otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}";
        }

        public bool ValidateCode(string secretKey, string code)
        {
            var key = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(key);
            return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2)); 
        }

        public string[] GenerateRecoveryCodes(int count = 10)
        {
            var codes = new string[count];
            for (int i = 0; i < count; i++)
            {
                codes[i] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(8))
                           .Replace("=", "").Replace("+", "").Replace("/", "");
            }
            return codes;
        }
    }
}