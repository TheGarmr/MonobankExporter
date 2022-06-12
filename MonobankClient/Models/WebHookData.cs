using System.Text.Json.Serialization;

namespace MonobankClient.Models
{
    public class WebHookData
    {
        [JsonPropertyName("account")]
        public string Account { get; set; }
        [JsonPropertyName("statementItem")]
        public Statement StatementItem { get; set; }
    }
}