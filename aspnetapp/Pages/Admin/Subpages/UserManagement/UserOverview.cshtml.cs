using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {

        public List<User> List { get; set; } = new List<User>();
        public IActionResult OnGet() {
            for (int i = 0; i < 10; i++) {
                List.Add(new User {
                    UserId = i,
                    Nickname = "a",
                    UserName = "b",
                    IdentityCard = "450000000000000123",
                    Phone = "13000000000"
                });
            }

            return Page();
        }
    }
}
