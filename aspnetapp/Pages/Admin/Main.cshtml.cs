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
        public void OnGet()
        {
        }

    }
}
