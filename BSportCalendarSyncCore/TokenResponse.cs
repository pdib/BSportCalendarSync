namespace BSportCalendarSyncCore
{
    using Newtonsoft.Json;

    internal class TokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
