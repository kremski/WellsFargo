using WellsFargo.DAL.Model;

namespace WellsFargo.Services
{
    public interface IDbTransactionService
    {
        Task<bool> Add(IEnumerable<Transaction> transactions);
        Task<IEnumerable<Transaction>> GetByOms(string omsCode);
    }
}
