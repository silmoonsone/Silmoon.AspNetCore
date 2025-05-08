using Newtonsoft.Json;
using System.Collections.Generic;

namespace Silmoon.AspNetCore.RequestModels
{
    public class AccessTokenErrorResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        public AccessTokenErrorResponse(TokenErrorCode errorCode)
        {
            var dict = ErrorDescriptions[errorCode];
            Error = dict.Key;
            ErrorDescription = dict.Value;
        }

        public static Dictionary<TokenErrorCode, KeyValuePair<string, string>> ErrorDescriptions = new Dictionary<TokenErrorCode, KeyValuePair<string, string>>()
        {
            { TokenErrorCode.InvalidRequest, new KeyValuePair<string, string>("invalid_request", "The request is missing a required parameter or is malformed.") },
            { TokenErrorCode.InvalidClient, new KeyValuePair<string, string>("invalid_client", "Client authentication failed.") },
            { TokenErrorCode.InvalidGrant, new KeyValuePair<string, string>("invalid_grant", "The provided authorization grant is invalid, expired, or revoked.") },
            { TokenErrorCode.UnauthorizedClient, new KeyValuePair<string, string>("unauthorized_client", "The client is not authorized to use this grant type.") },
            { TokenErrorCode.UnsupportedGrantType, new KeyValuePair<string, string>("unsupported_grant_type", "The authorization grant type is not supported by the authorization server.") },
            { TokenErrorCode.InvalidScope, new KeyValuePair<string, string>("invalid_scope", "The requested scope is invalid, unknown, or malformed.") },
        };
        public enum TokenErrorCode
        {
            InvalidRequest,
            InvalidClient,
            InvalidGrant,
            UnauthorizedClient,
            UnsupportedGrantType,
            InvalidScope
        }
    }
}
