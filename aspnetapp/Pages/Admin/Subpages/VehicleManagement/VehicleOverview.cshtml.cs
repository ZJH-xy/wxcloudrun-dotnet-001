using aspnetapp.Controllers.Web;
using aspnetapp.Pages.Admin.Subpages.UserManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.VehicleManagement {
    public class VehicleOverviewModel : PageModel {
        private readonly VehicleControllerWeb _vehicleController;
        private readonly ILogger<UserOverviewModel> _logger;

        public List<Models.Vehicle> List { get; set; } = new List<Models.Vehicle>();

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
        public async Task<IActionResult> OnGetAsync() {
            _logger.LogInformation("正在获取限制为{Limit}的页面{PageIndex}的用户列表", PageIndex, Limit);
            try {
                List = await _vehicleController.GetTablePage(Limit, PageIndex);

            } catch (Exception ex) {
                _logger.LogError(ex, "获取用户列表时出错");
                ErrorMessage = "加载用户列表时发生错误。";
                ModelState.AddModelError(string.Empty, "加载用户列表时发生错误。");
            }
            return Page();
        }

        public VehicleOverviewModel(VehicleControllerWeb vehicleController, ILogger<UserOverviewModel> logger) {
            _vehicleController = vehicleController;
            _logger = logger;
        }
    }
}
