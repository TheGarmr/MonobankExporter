using System.Text.Json.Serialization;

namespace MonobankClient.Models
{
    public class WebHook
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("data")]
        public WebHookData Data { get; set; }
    }
}
