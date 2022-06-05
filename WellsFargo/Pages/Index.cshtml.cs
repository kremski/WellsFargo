using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WellsFargo.Helpers;
using WellsFargo.Services;

namespace WellsFargo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IDbTransactionService _transactionService;
        private readonly ICsvParser _csvParser;
        public string? ExceptionMessage { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            IDbTransactionService transactionService,
            ICsvParser csvParser)
        {
            _logger = logger;
            _transactionService = transactionService;
            _csvParser = csvParser;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }
         
        public async Task OnPostAsync()
        {
            try
            {
                var transactions = _csvParser.GetTransactionListFromFile(Upload);
                if (transactions != null && transactions.Any())
                {
                    await _transactionService.Add(transactions);
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