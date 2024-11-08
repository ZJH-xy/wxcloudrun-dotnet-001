using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;
        private readonly ILogger<UserOverviewModel> _logger;

        public List<User> List { get; set; } = new List<User>();

        // 分页参数
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        // 用于绑定更新的用户信息
        [BindProperty]
        public User UpdatedUser { get; set; } = new User();

        public UserOverviewModel(UserControllerWeb userController, ILogger<UserOverviewModel> logger) {
            _userController = userController;
            _logger = logger;
        }

        // 获取分页用户列表
        public async Task<IActionResult> OnGetAsync() {
            _logger.LogInformation("Fetching user list for page {PageIndex} with limit {Limit}", PageIndex, Limit);
            try {
                List = await _userController.GetTablePage(Limit, PageIndex);
                _logger.LogInformation("Successfully fetched {Count} users", List.Count);
            } catch (Exception ex) {
                _logger.LogError(ex, "An error occurred while fetching the user list");
                ModelState.AddModelError(string.Empty, "Error loading user list.");
            }
            return Page();
        }

        // 更新用户信息
        public async Task<IActionResult> OnPostUpdateUserAsync() {
            _logger.LogInformation("正在尝试使用ID更新用户{UserId}", UpdatedUser.Id);

            if (!ModelState.IsValid) {
                _logger.LogWarning("找不到用户。{UserId}", UpdatedUser.Id);
                return Page();
            }

            var result = await _userController.UpdateUser(UpdatedUser);
            if (result is NotFoundResult) {
                _logger.LogWarning("User with ID {UserId} not found", UpdatedUser.Id);
                ModelState.AddModelError(string.Empty, "找不到用户。");

            } else if (result is ConflictResult) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突{UserId}", UpdatedUser.Id);
                ModelState.AddModelError(string.Empty, "您尝试编辑的记录已被其他用户修改。请重新加载数据，然后重试。");

            } else if (result is StatusCodeResult status && status.StatusCode == 500) {
                _logger.LogError("An error occurred while updating user with ID {UserId}", UpdatedUser.Id);
                ModelState.AddModelError(string.Empty, "更新用户时出错。");

            } else {
                _logger.LogInformation("User with ID {UserId} updated successfully", UpdatedUser.Id);
                TempData["Message"] = "User updated successfully.";
            }

            // 重新加载用户列表以更新页面显示
            List = await _userController.GetTablePage(Limit, PageIndex);
            return Page();
        }
    }
}
