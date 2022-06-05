namespace WellsFargo.DAL.Model
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public int PortfolioId { get; set; }
        public virtual Portfolio Portfolio { get; set; }

        public int SecurityId { get; set; }
        public virtual Security Security { get; set; }

        public decimal Nominal { get; set; }

        public int OmsId { get; set; }
        public virtual Oms Oms { get; set; }

        public int TransactionTypeId { get; set; }
        public virtual TransactionType TransactionType { get; set; }
    }
}
