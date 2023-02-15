namespace MonobankExporter.Application.Options;

public class ClientInfoOptions
{
    public string Token { get; set; }
    public string Name { get; set; }
    public string WebHookUrl { get; set; }

    public override string ToString()
    {
        return Name;
    }
}