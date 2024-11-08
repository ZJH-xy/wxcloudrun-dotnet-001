using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;

        public List<User> List { get; set; } = new List<User>();

        // 分页参数
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        // 用于绑定更新的用户信息
        [BindProperty]
        public User UpdatedUser { get; set; } = new User();

        public UserOverviewModel(UserControllerWeb userController) {
            _userController = userController;
        }

        // 获取分页用户列表
        public async Task<IActionResult> OnGetAsync() {
            List = await _userController.GetTablePage(Limit, PageIndex);
            return Page();
        }

        // 更新用户信息
        public async Task<IActionResult> OnPostUpdateUserAsync() {
            if (!ModelState.IsValid) {
                return Page();
            }

            var result = await _userController.UpdateUser(UpdatedUser);
            if (result is NotFoundResult) {
                ModelState.AddModelError(string.Empty, "用户不存在");
            } else if (result is StatusCodeResult status && status.StatusCode == 500) {
                ModelState.AddModelError(string.Empty, "错误");
            } else {
                TempData["Message"] = "更新成功";
            }

            // 重新加载用户列表以更新页面显示
            List = await _userController.GetTablePage(Limit, PageIndex);
            return Page();
        }
    }
}
