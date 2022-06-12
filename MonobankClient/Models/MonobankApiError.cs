using System.Text.Json.Serialization;

namespace MonobankClient.Models
{
    public class MonobankApiError
    {
        [JsonPropertyName("errorDescription")]
        public string Description { get; set; }
    }
}
