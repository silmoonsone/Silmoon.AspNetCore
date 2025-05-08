namespace Silmoon.AspNetCore.RequestModels
{
    public class AuthorizeRequest
    {
        public string response_type { get; set; }
        public string client_id { get; set; }
        public string redirect_uri { get; set; }
        public string scope { get; set; }
        public string state { get; set; }
        public string code_challenge { get; set; }
        public string code_challenge_method { get; set; }
        public string AppId { get; set; }
    }
}
