namespace MonobankExporter.BusinessLogic.Models
{
    public class AccountInfoModel
    {
        public string HolderName { get; set; }
        public string CurrencyType { get; set; }
        public string CardType { get; set; }
        public double CreditLimit { get; set; }
    }
}