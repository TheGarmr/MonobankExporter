namespace MonobankExporter.Application.Models
{
    public class AccountInfo
    {
        public string HolderName { get; set; }
        public string CurrencyType { get; set; }
        public string CardType { get; set; }
        public double CreditLimit { get; set; }
    }
}