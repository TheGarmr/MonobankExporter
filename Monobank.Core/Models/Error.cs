using System.Text.Json.Serialization;

namespace MonobankExporter.Client.Models
{
    public class Error
    {
        [JsonPropertyName("errorDescription")]
        public string Description { get; set; }
    }
}
