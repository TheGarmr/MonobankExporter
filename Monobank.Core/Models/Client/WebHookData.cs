using System.Text.Json.Serialization;

namespace MonobankExporter.Domain.Models.Client
{
    public class WebHookData
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }
        [JsonPropertyName("statementItem")]
        public Statement StatementItem { get; set; }
    }
}