using System.Text.Json.Serialization;

namespace MonobankExporter.Domain.Models.Client
{
    public class WebHook
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("data")]
        public WebHookData Data { get; set; }
    }
}
