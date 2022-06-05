namespace WellsFargo.DAL.Model
{
    public class Oms
    {
        public int OmsId { get; set; }
        public string OmsCode { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
