using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;

        public List<User> List { get; set; } = new List<User>();

        // ·ÖÒ³²ÎÊý
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public UserOverviewModel(UserControllerWeb userController) {
            _userController = userController;
        }

        public async Task<IActionResult> OnGetAsync() {
            List = await _userController.GetTablePage(Limit, PageIndex);
            return Page();
        }
    }
}
