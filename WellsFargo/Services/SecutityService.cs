using WellsFargo.DAL;
using WellsFargo.DAL.Model;

namespace WellsFargo.Services
{
    public class SecurityService : IDbService<Security>
    {
        private readonly WellsFargoDbContext _context;

        public SecurityService(WellsFargoDbContext context) { _context = context; }

        public async Task<bool> AddOrUpdate(IEnumerable<Security> securities)
        {
            try
            {
                foreach (var security in securities)
                {
                    var dbSecurity = await _context.Security.FindAsync(security.SecurityId);
                    if (dbSecurity != null)
                    {
                        dbSecurity.ISIN = security.ISIN;
                        dbSecurity.Ticker = security.Ticker;
                        dbSecurity.Cusip = security.Cusip;
                        _context.Security.Update(dbSecurity);
                    }
                    else
                    {
                        security.SecurityId = 0;
                        _context.Security.Add(security);
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
