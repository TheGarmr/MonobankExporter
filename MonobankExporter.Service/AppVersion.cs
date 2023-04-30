namespace MonobankExporter.Service;

public class AppVersion
{
    public string Version => GetVersion();

    private string GetVersion()
    {
        var version = GetType().Assembly.GetName().Version;
        return $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    public override string ToString()
    {
        return Version;
    }

}