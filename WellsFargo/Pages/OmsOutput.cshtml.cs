using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using WellsFargo.Helpers;
using WellsFargo.Services;

namespace WellsFargo.Pages
{
    public class OmsOutputModel : PageModel
    {
        [BindProperty] public string OmsOutout { get; set; }
        public List<SelectListItem> OmsOptions { get; set; }

        private readonly IDbTransactionService _transactionService;
        private readonly IOmsOutoutHelper _omsOutoutHelper;

        public OmsOutputModel(
            IDbTransactionService transactionService,
            IOmsOutoutHelper omsOutoutHelper)
        {
            _transactionService = transactionService;
            _omsOutoutHelper = omsOutoutHelper;
        }

        public void OnGet()
        {
            OmsOptions = new List<SelectListItem>
            {
                new SelectListItem{ Value = null, Text = "Select an OMS", Selected = true },
                new SelectListItem{ Value = "AAA", Text = "AAA" },
                new SelectListItem{ Value = "BBB", Text = "BBB" },
                new SelectListItem{ Value = "CCC", Text = "CCC" }
            };
        }

        public async Task<FileResult> OnPost()
        {
            var omsTransactions = await _transactionService.GetByOms(OmsOutout);
            if (omsTransactions != null && omsTransactions.Any())
            {
                var csv = _omsOutoutHelper.GetOmsOutput(omsTransactions, OmsOutout);
                return File
                    (Encoding.UTF8.GetBytes(csv),
                    "text/csv",
                    $"{DateTime.Now.ToString("yyyy-MM-dd")}T{DateTime.Now.ToString("hhmmss")}.{OmsOutout.ToLowerInvariant()}");
            }
            return null;
        }
    }
}
