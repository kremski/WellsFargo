namespace WellsFargo.DAL.Model
{
    public class TransactionType
    {
        public int TransactionTypeId { get; set; }
        public string TransactionTypeCode { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
