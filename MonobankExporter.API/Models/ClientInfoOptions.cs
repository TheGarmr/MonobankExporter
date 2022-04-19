namespace MonobankExporter.API.Models
{
    public class ClientInfoOptions
    {
        public string Token { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}