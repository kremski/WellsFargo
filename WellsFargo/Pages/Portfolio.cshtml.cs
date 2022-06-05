using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WellsFargo.DAL.Model;
using WellsFargo.Helpers;
using WellsFargo.Services;

namespace WellsFargo.Pages
{
    public class PortfolioModel : PageModel
    {
        private readonly ILogger<PortfolioModel> _logger;
        private readonly IDbService<Portfolio> _portfolioService;
        private readonly ICsvParser _csvParser;

        public PortfolioModel(
            ILogger<PortfolioModel> logger,
            IDbService<Portfolio> portfolioService,
            ICsvParser csvParser)
        {
            _logger = logger;
            _portfolioService = portfolioService;
            _csvParser = csvParser;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task OnPostAsync()
        {
            try 
            { 
                var portfolios = _csvParser.GetPortfolioListFromFile(Upload);
                if (portfolios != null && portfolios.Any())
                {
                    await _portfolioService.AddOrUpdate(portfolios);
                }
                TempData["success"] = "File uploaded successfuly";
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                ModelState.AddModelError(string.Empty, message);
            }
        }   
    }
}