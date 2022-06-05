using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WellsFargo.DAL;
using WellsFargo.DAL.Model;

namespace WellsFargo.Services
{
    public class DbTransactionService : IDbTransactionService
    {
        private readonly WellsFargoDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public DbTransactionService(
            WellsFargoDbContext context,
            IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<bool> Add(IEnumerable<Transaction> transactions)
        {
            try
            {
                await _context.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Transaction>> GetByOms(string omsCode)
        {
            if (!_memoryCache.TryGetValue(omsCode, out int omsId))
            {
                var oms = await _context.Oms.AsNoTracking().SingleOrDefaultAsync(x => x.OmsCode.Equals(omsCode.ToUpperInvariant()));
                if (oms != null)
                {
                    _memoryCache.Set(oms.OmsCode, oms.OmsId);
                    omsId = oms.OmsId;
                }
            }

            return _context.Transaction
                .AsNoTracking()
                .Where(x => x.OmsId == omsId)
                .Include(x => x.Security)
                .Include(x => x.Portfolio)
                .Include(x => x.TransactionType)
                .Include(x => x.Oms)
                .ToList();
        }
    }
}
