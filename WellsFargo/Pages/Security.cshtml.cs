using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WellsFargo.DAL.Model;
using WellsFargo.Helpers;
using WellsFargo.Services;

namespace WellsFargo.Pages
{
    public class SecurityModel : PageModel
    {
        private readonly ILogger<SecurityModel> _logger;
        private readonly IDbService<Security> _securityService;
        private readonly ICsvParser _csvParser;

        public SecurityModel(
            ILogger<SecurityModel> logger,
            IDbService<Security> securityService,
            ICsvParser csvParser)
        {
            _logger = logger;
            _securityService = securityService;
            _csvParser = csvParser;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public async Task OnPostAsync()
        {
            try
            {
                var securities = _csvParser.GetSecurityListFromFile(Upload);
                if (securities != null && securities.Any())
                {
                    await _securityService.AddOrUpdate(securities);
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