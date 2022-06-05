using AutoMapper;
using System.Text;
using WellsFargo.DAL.Model;
using WellsFargo.Helpers.OmsOutputModels;

namespace WellsFargo.Helpers
{
    public class OmsOutoutHelper : IOmsOutoutHelper
    {
        private readonly IMapper _mapper;

        public OmsOutoutHelper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public string GetOmsOutput(IEnumerable<Transaction> transactions, string omsCode)
        {
            var csv = new StringBuilder();
            var header = string.Empty;
            var delimiter = string.Empty;

            // map trans to omsOutputs
            switch (omsCode)
            {
                case "AAA":
                    header = "ISIN,PortfolioCode,Nominal,TransactionType";
                    delimiter = ",";

                    var omsOutputAaaModels = transactions.Select(x => _mapper.Map<OmsOutputAaaModel>(x)).ToList();

                    if (!string.IsNullOrEmpty(header))
                        csv.AppendLine(header);

                    foreach (var item in omsOutputAaaModels)
                    {
                        csv.AppendLine(
                            item.Isin
                            + delimiter
                            + item.PortfolioCode
                            + delimiter
                            + item.Nominal
                            + delimiter
                            + item.TransactionType);
                    }
                    return csv.ToString();

                case "BBB":
                    header = "Cusip|PortfolioCode|Nominal|TransactionType";
                    delimiter = "|";

                    var omsOutputBbbModels = transactions.Select(x => _mapper.Map<OmsOutputBbbModel>(x)).ToList();

                    if (!string.IsNullOrEmpty(header))
                        csv.AppendLine(header);

                    foreach (var item in omsOutputBbbModels)
                    {
                        csv.AppendLine(
                            item.Cusip
                            + delimiter
                            + item.PortfolioCode
                            + delimiter
                            + item.Nominal
                            + delimiter
                            + item.TransactionType);
                    }
                    return csv.ToString();

                case "CCC":
                    delimiter = ",";

                    var omsOutputCccModels = transactions.Select(x => _mapper.Map<OmsOutputCccModel>(x)).ToList();

                    if (!string.IsNullOrEmpty(header))
                        csv.AppendLine(header);

                    foreach (var item in omsOutputCccModels)
                    {
                        csv.AppendLine(
                            item.PortfolioCode
                            + delimiter
                            + item.Ticker
                            + delimiter
                            + item.Nominal
                            + delimiter
                            + item.TransactionType);
                    }
                    return csv.ToString();

                default:
                    return string.Empty;
            }
        }
    }
}
