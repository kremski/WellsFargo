namespace WellsFargo.Helpers.OmsOutputModels
{
    public class OmsOutputBaseModel
    {
        public int TransactionId { get; set; }
        public decimal Nominal { get; set; }
        public string PortfolioCode { get; set; }
        public string TransactionType { get; set; }
    }
}