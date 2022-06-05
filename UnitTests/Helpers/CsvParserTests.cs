using Moq;
using Microsoft.Extensions.Caching.Memory;
using WellsFargo.DAL.Model;
using WellsFargo.DAL;
using Microsoft.AspNetCore.Http;
using WellsFargo.Helpers;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests.Helpers
{
    public class CsvParserTests
    {
        private WellsFargoDbContext mockContext;
        private List<Portfolio> portfolios;
        private List<Oms> oms;
        private List<TransactionType> transactionTypes;
        private List<Security> securities;

        private CsvParser _csvParser;

        public CsvParserTests()
        {
            var dbOptions = new DbContextOptionsBuilder<WellsFargoDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;
            mockContext = new WellsFargoDbContext(dbOptions);

            portfolios = new List<Portfolio>
            {
                new Portfolio { PortfolioId = 1, PortfolioCode = "p1" },
                new Portfolio { PortfolioId = 2, PortfolioCode = "p2" }
            };
            var queryablePortfolio = portfolios.AsQueryable();
            var dbSetPortfolio = new Mock<DbSet<Portfolio>>();
            dbSetPortfolio.As<IQueryable<Portfolio>>().Setup(m => m.Provider).Returns(queryablePortfolio.Provider);
            dbSetPortfolio.As<IQueryable<Portfolio>>().Setup(m => m.Expression).Returns(queryablePortfolio.Expression);
            dbSetPortfolio.As<IQueryable<Portfolio>>().Setup(m => m.ElementType).Returns(queryablePortfolio.ElementType);
            dbSetPortfolio.As<IQueryable<Portfolio>>().Setup(m => m.GetEnumerator()).Returns(() => queryablePortfolio.GetEnumerator());
            mockContext.Portfolio = dbSetPortfolio.Object;

            securities = new List<Security>
            {
                new Security { SecurityId = 1, ISIN = "ISIN11111111", Ticker = "s1", Cusip = "CUSIP0001" },
                new Security { SecurityId = 2, ISIN = "ISIN22222222", Ticker = "s2", Cusip = "CUSIP0002" }
            };
            var queryableSecurity = securities.AsQueryable();
            var dbSetSecurity = new Mock<DbSet<Security>>();
            dbSetSecurity.As<IQueryable<Security>>().Setup(m => m.Provider).Returns(queryableSecurity.Provider);
            dbSetSecurity.As<IQueryable<Security>>().Setup(m => m.Expression).Returns(queryableSecurity.Expression);
            dbSetSecurity.As<IQueryable<Security>>().Setup(m => m.ElementType).Returns(queryableSecurity.ElementType);
            dbSetSecurity.As<IQueryable<Security>>().Setup(m => m.GetEnumerator()).Returns(() => queryableSecurity.GetEnumerator());
            mockContext.Security = dbSetSecurity.Object;

            oms = new List<Oms> {
                new Oms { OmsId = 1, OmsCode = "AAA" },
                new Oms { OmsId = 2, OmsCode = "BBB" },
                new Oms { OmsId = 3, OmsCode = "CCC" }
            };
            var queryableOms = oms.AsQueryable();
            var dbSetOms = new Mock<DbSet<Oms>>();
            dbSetOms.As<IQueryable<Oms>>().Setup(m => m.Provider).Returns(queryableOms.Provider);
            dbSetOms.As<IQueryable<Oms>>().Setup(m => m.Expression).Returns(queryableOms.Expression);
            dbSetOms.As<IQueryable<Oms>>().Setup(m => m.ElementType).Returns(queryableOms.ElementType);
            dbSetOms.As<IQueryable<Oms>>().Setup(m => m.GetEnumerator()).Returns(() => queryableOms.GetEnumerator());
            mockContext.Oms = dbSetOms.Object;

            transactionTypes = new List<TransactionType>
            {
                new TransactionType { TransactionTypeId = 1, TransactionTypeCode = "BUY" },
                new TransactionType { TransactionTypeId = 2, TransactionTypeCode = "SELL" }
            };
            var queryableTransactionType = transactionTypes.AsQueryable();
            var dbSetTransactionType = new Mock<DbSet<TransactionType>>();
            dbSetTransactionType.As<IQueryable<TransactionType>>().Setup(m => m.Provider).Returns(queryableTransactionType.Provider);
            dbSetTransactionType.As<IQueryable<TransactionType>>().Setup(m => m.Expression).Returns(queryableTransactionType.Expression);
            dbSetTransactionType.As<IQueryable<TransactionType>>().Setup(m => m.ElementType).Returns(queryableTransactionType.ElementType);
            dbSetTransactionType.As<IQueryable<TransactionType>>().Setup(m => m.GetEnumerator()).Returns(() => queryableTransactionType.GetEnumerator());
            mockContext.TransactionType = dbSetTransactionType.Object;

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();
            var memoryCache = serviceProvider.GetService<IMemoryCache>();

            _csvParser = new CsvParser(
                mockContext,
                memoryCache);
        }

        [Fact]
        public void GetPortfolioListFromFile_returns_list()
        {
            //arrange
            var content = "PortfolioId,PortfolioCode\r\n1,p1\r\n2,p3";
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            var result = _csvParser.GetPortfolioListFromFile(file);

            //assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetPortfolioListFromFile_returns_emptyList()
        {
            //arrange
            var content = "Portfolio";
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            var result = _csvParser.GetPortfolioListFromFile(file);

            //assert
            Assert.Equal(new List<Portfolio>(), result);
        }

        [Fact]
        public void GetPortfolioListFromFile_throwsValidationException_EmptyFile()
        {
            //arrange
            var content = string.Empty;
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            Action action = () => _csvParser.GetPortfolioListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("File is empty.", exception.Message);
        }

        [Fact]
        public void GetPortfolioListFromFile_throwsValidationException_ColumnCount()
        {
            //arrange
            var content = "Portfolio\r\nvalue";
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            Action action = () => _csvParser.GetPortfolioListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Missing data at line 2. Must be 2 non empty columns.", exception.Message);
        }

        [Fact]
        public void GetPortfolioListFromFile_throwsValidationException_ParseInt()
        {
            //arrange
            var content = "PortfolioId,PortfolioCode\r\na,p1\r\n2,p3";
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            Action action = () => _csvParser.GetPortfolioListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Failed to read PortfolioId at line 2. Must be a valid integer.", exception.Message);
        }

        [Fact]
        public void GetPortfolioListFromFile_throwsValidationException_EmptyPortfolioCode()
        {
            //arrange
            var content = "PortfolioId,PortfolioCode\r\n1,\r\n2,p3";
            var fileName = "portfolio.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "portfolio", fileName);

            //act
            Action action = () => _csvParser.GetPortfolioListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("PortfolioCode too short at line 2. Must have a value.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_returns_list()
        {
            //arrange
            var content = "SecurityId,ISIN,Ticker,CUSIP\r\n1,ISIN11111111,s1,CUSIP0001\r\n2,ISIN22222222,s2,CUSIP0002";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            var result = _csvParser.GetSecurityListFromFile(file);

            //assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetSecurityListFromFile_returns_emptyList()
        {
            //arrange
            var content = "Security";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            var result = _csvParser.GetSecurityListFromFile(file);

            //assert
            Assert.Equal(new List<Security>(), result);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_EmptyFile()
        {
            //arrange
            var content = string.Empty;
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("File is empty.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_ColumnCount()
        {
            //arrange
            var content = "SecurityId\r\n1";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Missing data at line 2. Must be 4 non empty columns.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_ParseInt()
        {
            //arrange
            var content = "SecurityId,ISIN,Ticker,CUSIP\r\na,ISIN11111111,s1,CUSIP0001\r\n2,ISIN22222222,s2,CUSIP0002";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Failed to read SecurityId at line 2. Must be a valid integer.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_IsinEmpty()
        {
            //arrange
            var content = "SecurityId,ISIN,Ticker,CUSIP\r\n1,,s1,CUSIP0001\r\n2,ISIN22222222,s2,CUSIP0002";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("ISIN too short at line 2. Must have a value.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_TickerEmpty()
        {
            //arrange
            var content = "SecurityId,ISIN,Ticker,CUSIP\r\n1,ISIN11111111,,CUSIP0001\r\n2,ISIN22222222,s2,CUSIP0002";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Ticker too short at line 2. Must have a value.", exception.Message);
        }

        [Fact]
        public void GetSecurityListFromFile_throwsValidationException_CusipEmpty()
        {
            //arrange
            var content = "SecurityId,ISIN,Ticker,CUSIP\r\n1,ISIN11111111,s1,\r\n2,ISIN22222222,s2,CUSIP0002";
            var fileName = "security.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "security", fileName);

            //act
            Action action = () => _csvParser.GetSecurityListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Cusip too short at line 2. Must have a value.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_returns_list()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n1,1,11,AAA,BUY\r\n2,2,22,BBB,SELL\r\n1,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            var result = _csvParser.GetTransactionListFromFile(file);

            //assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public void GetTransactionListFromFile_returns_emptyList()
        {
            //arrange
            var content = "SecurityIdY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            var result = _csvParser.GetTransactionListFromFile(file);

            //assert
            Assert.Equal(new List<Transaction>(), result);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_EmptyFile()
        {
            //arrange
            var content = string.Empty;
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("File is empty.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_ColumnCount()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n1,11,AAA,BUY\r\n2,22,BBB,SELL\r\n2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Missing data at line 2. Must be 5 non empty columns.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_ParseIntSecurity()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\na,1,11,AAA,BUY\r\nb,2,22,BBB,SELL\r\na,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Failed to read SecurityId at line 2. Must be a valid integer.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_InvalidNominal()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n1,1,nominal,AAA,BUY\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Failed to read Nominal at line 2. Must be a valid decimal.", exception.Message);
        }
        
        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_InvalidSecurityId()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n3,1,11,AAA,BUY\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Invalid SecurityId at line 2. Ensure valid entry.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_InvalidLengthOms()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n2,1,11,ABCD,BUY\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("OMS incorrect length at line 2. Must be 3 characters.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_InvalidOms()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n2,1,11,DDD,BUY\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Invalid OMS at line 2. Ensure valid entry.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_EmptyTransactionType()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n2,1,11,AAA,\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("TransactionType too short at line 2. Must have a value.", exception.Message);
        }

        [Fact]
        public void GetTransactionListFromFile_throwsValidationException_InvalidTransactionType()
        {
            //arrange
            var content = "SecurityId,PortfolioId,Nominal,OMS,TransactionType\r\n2,1,11,AAA,TEST\r\n1,2,22,BBB,SELL\r\n2,2,33,CCC,BUY";
            var fileName = "transactions.csv";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;
            IFormFile file = new FormFile(stream, 0, stream.Length, "transaction", fileName);

            //act
            Action action = () => _csvParser.GetTransactionListFromFile(file);

            //assert
            ValidationException exception = Assert.Throws<ValidationException>(action);
            Assert.Equal("Invalid TransactionType at line 2. Ensure valid entry.", exception.Message);
        }
    }
}