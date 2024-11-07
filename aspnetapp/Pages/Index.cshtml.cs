using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages {
    public class IndexModel : PageModel {
        public RedirectToPageResult OnGet() {
            return RedirectToPage("./Admin/login");
        }
    }
}
