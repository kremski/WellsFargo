using System.ComponentModel.DataAnnotations;
using WellsFargo.DAL.Model;
using Microsoft.Extensions.Caching.Memory;
using WellsFargo.DAL;
using Microsoft.EntityFrameworkCore;

namespace WellsFargo.Helpers
{
    public class CsvParser : ICsvParser
    {
        private bool _isFirst;
        private int _lineNumber;

        private readonly WellsFargoDbContext _context;
        private readonly IMemoryCache _memoryCache; 
        // for simplifacation, assumption made: OMS and TransactionType will not ovelap (as in names won't be shared)

        public CsvParser(
            WellsFargoDbContext context,
            IMemoryCache memoryCache)
        {
            _isFirst = true;
            _lineNumber = 0;

            _context = context;
            _memoryCache = memoryCache;
        }

        public IEnumerable<Portfolio> GetPortfolioListFromFile(IFormFile upload)
        {
            try
            {
                _isFirst = true;
                _lineNumber = 0;

                IEnumerable<string> portfolios = upload.GetFromCsv();
                if (portfolios == null || !portfolios.Any())
                    throw new ValidationException("File is empty.");

                List<Portfolio> result = new();
                foreach (string portfolio in portfolios)
                {
                    var parsedPortfolio = parsePortfolioLine(portfolio);
                    if (parsedPortfolio != null)
                    {
                        result.Add(parsedPortfolio);
                    }
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }
        private Portfolio parsePortfolioLine(string line)
        {
            //Try to read header
            if (_isFirst && string.IsNullOrEmpty(line))
                throw new ValidationException("Failed to read file header.");

            _lineNumber++;
            if (_isFirst)
            {
                _isFirst = false;
                return null;
            }

            string[] lineData = line.Split(',');
            if (lineData.Length != 2)
                throw new ValidationException($"Missing data at line {_lineNumber}. Must be 2 non empty columns.");

            //Try to set id
            if (!int.TryParse(lineData[0], out int portfolioId))
                throw new ValidationException($"Failed to read PortfolioId at line {_lineNumber}. Must be a valid integer.");

            //Validate string field
            if (lineData[1].Length < 1)
                throw new ValidationException($"PortfolioCode too short at line {_lineNumber}. Must have a value.");

            return new Portfolio()
            {
                PortfolioId = portfolioId,
                PortfolioCode = lineData[1]
            };
        }

        public IEnumerable<Security> GetSecurityListFromFile(IFormFile upload)
        {
            try
            {
                _isFirst = true;
                _lineNumber = 0;

                IEnumerable<string> secutities = upload.GetFromCsv();
                if (secutities == null || !secutities.Any())
                    throw new ValidationException("File is empty.");

                List<Security> result = new();
                foreach (string security in secutities)
                {
                    var parsedSecurity = parseSecurityLine(security);
                    if (parsedSecurity != null)
                    {
                        result.Add(parsedSecurity);
                    }
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }
        private Security parseSecurityLine(string line)
        {
            //Try to read header
            if (_isFirst && string.IsNullOrEmpty(line))
                throw new ValidationException("Failed to read file header.");

            _lineNumber++;
            if (_isFirst)
            {
                _isFirst = false;
                return null;
            }

            string[] lineData = line.Split(',');
            if (lineData.Length != 4)
                throw new ValidationException($"Missing data at line {_lineNumber}. Must be 4 non empty columns.");

            //Try to set id
            if (!int.TryParse(lineData[0], out int securityId))
                throw new ValidationException($"Failed to read SecurityId at line {_lineNumber}. Must be a valid integer.");

            //Validate string fields
            if (lineData[1].Length < 1)
                throw new ValidationException($"ISIN too short at line {_lineNumber}. Must have a value.");
            if (lineData[2].Length < 1)
                throw new ValidationException($"Ticker too short at line {_lineNumber}. Must have a value.");
            if (lineData[3].Length < 1)
                throw new ValidationException($"Cusip too short at line {_lineNumber}. Must have a value.");

            return new Security()
            {
                SecurityId = securityId,
                ISIN = lineData[1],
                Ticker = lineData[2],
                Cusip = lineData[3]
            };
        }

        public IEnumerable<Transaction> GetTransactionListFromFile(IFormFile upload)
        {
            try
            {
                _isFirst = true;
                _lineNumber = 0;

                IEnumerable<string> transactions = upload.GetFromCsv();
                if (transactions == null || !transactions.Any())
                    throw new ValidationException("File is empty.");

                List<Transaction> result = new();
                foreach (string transaction in transactions)
                {
                    var parsedTransaction = parseTransactionLine(transaction);
                    if (parsedTransaction != null)
                    {
                        result.Add(parsedTransaction);
                    }
                }

                return result;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }
        private Transaction parseTransactionLine(string line)
        {
            //Try to read header
            if (_isFirst && string.IsNullOrEmpty(line))
                throw new ValidationException("Failed to read file header.");

            _lineNumber++;
            if (_isFirst)
            {
                _isFirst = false;
                return null;
            }

            string[] lineData = line.Split(',');
            if (lineData.Length != 5)
                throw new ValidationException($"Missing data at line {_lineNumber}. Must be 5 non empty columns.");

            if (!int.TryParse(lineData[0], out int securityId))
                throw new ValidationException($"Failed to read SecurityId at line {_lineNumber}. Must be a valid integer.");
            if (_context.Security.AsNoTracking().SingleOrDefault(x => x.SecurityId == securityId) == null)
                throw new ValidationException($"Invalid SecurityId at line {_lineNumber}. Ensure valid entry.");
            
            if (!int.TryParse(lineData[1], out int portfolioId))
                throw new ValidationException($"Failed to read PortfolioId at line {_lineNumber}. Must be a valid integer.");
            if (_context.Portfolio.AsNoTracking().SingleOrDefault(x => x.PortfolioId == portfolioId) == null)
                throw new ValidationException($"Invalid PortfolioId at line {_lineNumber}. Ensure valid entry.");
            
            if (!decimal.TryParse(lineData[2], out decimal nominal))
                throw new ValidationException($"Failed to read Nominal at line {_lineNumber}. Must be a valid decimal.");

            if (lineData[3].Length != 3)
                throw new ValidationException($"OMS incorrect length at line {_lineNumber}. Must be 3 characters.");
            if (!_memoryCache.TryGetValue(lineData[3], out int omsId))
            {
                var oms = _context.Oms.AsNoTracking().SingleOrDefault(x => x.OmsCode.Equals(lineData[3].ToUpperInvariant()));
                if (oms != null)
                {
                    _memoryCache.Set(oms.OmsCode, oms.OmsId);
                    omsId = oms.OmsId;
                }
                else
                    throw new ValidationException($"Invalid OMS at line {_lineNumber}. Ensure valid entry.");
            }

            if (lineData[4].Length < 1)
                throw new ValidationException($"TransactionType too short at line {_lineNumber}. Must have a value.");
            if (!_memoryCache.TryGetValue(lineData[4], out int transactionTypeId))
            {
                var transactionType = _context.TransactionType.AsNoTracking().SingleOrDefault(x => x.TransactionTypeCode.Equals(lineData[4].ToUpperInvariant()));
                if (transactionType != null)
                {
                    _memoryCache.Set(transactionType.TransactionTypeCode, transactionType.TransactionTypeId);
                    transactionTypeId = transactionType.TransactionTypeId;
                }
                else
                    throw new ValidationException($"Invalid TransactionType at line {_lineNumber}. Ensure valid entry.");
            }

            return new Transaction()
            {
                SecurityId = securityId,
                PortfolioId = portfolioId,
                Nominal = nominal,
                OmsId = omsId,
                TransactionTypeId = transactionTypeId
            };
        }
    }
}
