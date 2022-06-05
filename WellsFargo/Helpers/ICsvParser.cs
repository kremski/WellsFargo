using WellsFargo.DAL.Model;

namespace WellsFargo.Helpers
{
    public interface ICsvParser
    {
        IEnumerable<Portfolio> GetPortfolioListFromFile(IFormFile upload);
        IEnumerable<Security> GetSecurityListFromFile(IFormFile upload);
        IEnumerable<Transaction> GetTransactionListFromFile(IFormFile upload);
    }
}