namespace WellsFargo.DAL.Model
{
    public class Security
    {
        public int SecurityId { get; set; }
        public string ISIN { get; set; }
        public string Ticker { get; set; }
        public string Cusip { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
