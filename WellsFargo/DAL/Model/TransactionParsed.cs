namespace WellsFargo.DAL.Model
{
    public class TransactionParsed
    {
        public int PortfolioId { get; set; }
        public int SecurityId { get; set; }
        public decimal Nominal { get; set; }
        public string OmsParsed { get; set; }
        public string TransactionTypeParsed { get; set; }
    }
}
