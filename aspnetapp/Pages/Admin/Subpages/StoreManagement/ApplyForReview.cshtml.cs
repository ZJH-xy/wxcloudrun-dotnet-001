using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.StoreManagement
{
    public class ApplyForReviewModel : PageModel
    {
        public struct User
        {
            public int UserId;
            public string Nickname;
            public string UserName;
            public string IdentityCard;
            public string Phone;

        }
        public List<User> List { get; set; } = new List<User>();
        public IActionResult OnGet()
        {
            for (int i = 0; i < 10; i++)
            {
                List.Add(new User
                {
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
