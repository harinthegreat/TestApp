namespace SocialMediaBackend.DTOs
{
    public class EnableMfaRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class VerifyMfaRequest
    {
        public string Code { get; set; } = string.Empty;
    }

    public class MfaSetupResponse
    {
        public string SecretKey { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public string[] RecoveryCodes { get; set; } = Array.Empty<string>();
    }
}