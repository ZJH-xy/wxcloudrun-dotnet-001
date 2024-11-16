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
        public async Task<IActionResult> OnPostAddVehicleAsync([FromForm] Models.Vehicle newVehicle)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "输入数据无效，请检查。";
                    return Page();
                }

                // 调用控制器添加数据
                await _vehicleController.AddVehicle(newVehicle);

                // 添加成功后记录日志并显示成功消息
                _logger.LogInformation("新增车辆成功：{Vehicle}", newVehicle);
                SuccessMessage = "车辆添加成功。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增车辆时发生错误");
                ErrorMessage = "添加车辆时发生错误，请稍后重试。";
                ModelState.AddModelError(string.Empty, "添加车辆时发生错误。");
            }

            // 刷新页面以显示新增数据
            return RedirectToPage(new { PageIndex, Limit });
        }


        public VehicleOverviewModel(VehicleControllerWeb vehicleController, ILogger<UserOverviewModel> logger) {
            _vehicleController = vehicleController;
            _logger = logger;
        }
    }
}
