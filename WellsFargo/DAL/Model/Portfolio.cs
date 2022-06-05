namespace WellsFargo.DAL.Model
{
    public class Portfolio
    {
        public int PortfolioId { get; set; }
        public string PortfolioCode { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
