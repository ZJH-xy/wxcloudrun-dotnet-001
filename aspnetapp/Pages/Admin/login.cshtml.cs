using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin
{
    public class LoginModel : PageModel {
        [BindProperty]
        public UserViewModel User { get; set; }

        public IActionResult OnGet() {
            return Page();
        }

        public IActionResult OnPost(UserViewModel model) {
            if (!ModelState.IsValid) {
                return Page();
            }

            ViewData["Email"] = User.Email;

            return Page();
        }

        [BindProperties(SupportsGet = true)]
        public class UserViewModel {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}