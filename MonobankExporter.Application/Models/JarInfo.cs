namespace MonobankExporter.Application.Models;

public class JarInfo
{
    public string HolderName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public double Balance { get; set; }
    public double? Goal { get; set; }
    public string CurrencyType { get; set; }
}