using WellsFargo.DAL;
using WellsFargo.DAL.Model;

namespace WellsFargo.Services
{
    public class PortfolioService : IDbService<Portfolio>
    {
        private readonly WellsFargoDbContext _context;

        public PortfolioService(WellsFargoDbContext context) { _context = context; }

        public async Task<bool> AddOrUpdate(IEnumerable<Portfolio> portfolios)
        {
            try
            {
                foreach (var portfolio in portfolios)
                {
                    var dbPortfolio = await _context.Portfolio.FindAsync(portfolio.PortfolioId);
                    if (dbPortfolio != null)
                    {
                        dbPortfolio.PortfolioCode = portfolio.PortfolioCode;
                        _context.Portfolio.Update(dbPortfolio);
                    }
                    else
                    {
                        portfolio.PortfolioId = 0;
                        _context.Portfolio.Add(portfolio);
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
