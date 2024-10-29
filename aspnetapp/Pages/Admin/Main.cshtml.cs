using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin
{
    public class homeModel : PageModel
    {
        public struct User
        {
            public string Name;
            public int Age;
        }
        public List<User> List { get; set; } = new List<User>();
        public IActionResult OnGet()
        {
            for(int i = 0; i < 10; i++)
            {
                List.Add(new User
                {
                    Name = "aaa",
                    Age = 18+ i
                });
            }

            return Page();
        }

    }
}
