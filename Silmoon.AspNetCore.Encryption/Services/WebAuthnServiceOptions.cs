namespace Silmoon.AspNetCore.Encryption.Services
{
    public class WebAuthnServiceOptions
    {
        public string AppName { get; set; } = "LocalhostTest";
        public string Host { get; set; } = null;
        public string GetWebAuthnOptionsUrl { get; set; } = "/_webAuthn/getWebAuthnOptions";
        public string CreateWebAuthnUrl { get; set; } = "/_webAuthn/createWebAuthn";
        public string DeleteWebAuthnUrl { get; set; } = "/_webAuthn/deleteWebAuthn";
        public string GetWebAuthnAuthenticateOptions { get; set; } = "/_webAuthn/getWebAuthnAuthenticateOptions";
        public string AuthenticateWebAuthn { get; set; } = "/_webAuthn/authenticateWebAuthn";
    }
}
