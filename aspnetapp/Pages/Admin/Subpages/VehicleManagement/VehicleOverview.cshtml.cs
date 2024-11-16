using aspnetapp.Controllers.Web;
using aspnetapp.Pages.Admin.Subpages.UserManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.VehicleManagement {
    public class VehicleOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;
        private readonly ILogger<UserOverviewModel> _logger;

        public List<Vehicle> List { get; set; } = new List<Vehicle>();

        // 用于在页面显示信息
        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }

        // 分页查询
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]

        public int PageIndex { get; set; } = 1;
        public struct Vehicle {

        }

        public VehicleOverviewModel(UserControllerWeb userController, ILogger<UserOverviewModel> logger) {
            _userController = userController;
            _logger = logger;
        }


        public IActionResult OnGet() {


            return Page();
        }
    }
}
