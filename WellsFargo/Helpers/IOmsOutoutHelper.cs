using WellsFargo.DAL.Model;

namespace WellsFargo.Helpers
{
    public interface IOmsOutoutHelper
    {
        string GetOmsOutput(IEnumerable<Transaction> transactions, string omsCode);
    }
}
