using System.Text.Json.Serialization;

namespace MonobankExporter.Domain.Models.Client
{
    public class MonobankApiError
    {
        [JsonPropertyName("errorDescription")]
        public string Description { get; set; }
    }
}
