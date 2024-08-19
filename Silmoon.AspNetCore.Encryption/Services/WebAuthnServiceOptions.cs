namespace Silmoon.AspNetCore.Encryption.Services
{
    public class WebAuthnServiceOptions
    {
        public string AppName { get; set; } = "LocalhostTest";
        public string Host { get; set; } = "localhost";
        public string GetWebAuthnOptionsUrl { get; set; } = "/getWebAuthnOptions";
        public string CreateWebAuthnUrl { get; set; } = "/createWebAuthn";
        public string DeleteWebAuthnUrl { get; set; } = "/deleteWebAuthn";
        public string GetWebAuthnAssertionOptions { get; set; } = "/getWebAuthnAssertionOptions";
        public string VerifyWebAuthnUrl { get; set; } = "/verifyWebAuthn";
    }
}
