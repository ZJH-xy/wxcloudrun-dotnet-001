using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement
{
    public class UserOverviewModel : PageModel
    {
        private readonly UserControllerWeb _userController;
        private readonly ILogger<UserOverviewModel> _logger;

        public List<User> List { get; set; } = new List<User>();

        // 用于在页面显示错误信息
        public string ErrorMessage { get; set; }

        // 用于在页面显示成功信息
        public string SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        [BindProperty]
        public User UpdatedUser { get; set; } = new User();

        public UserOverviewModel(UserControllerWeb userController, ILogger<UserOverviewModel> logger)
        {
            _userController = userController;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Fetching user list for page {PageIndex} with limit {Limit}", PageIndex, Limit);
            try
            {
                List = await _userController.GetTablePage(Limit, PageIndex);
                _logger.LogInformation("Successfully fetched {Count} users", List.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the user list");
                ErrorMessage = "加载用户列表时发生错误。";
                ModelState.AddModelError(string.Empty, "加载用户列表时发生错误。");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateUserAsync()
        {
            _logger.LogInformation("正在尝试使用ID更新用户{UserId}", UpdatedUser.Id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("表单验证失败。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "表单验证失败，请检查输入内容";
                return Page();
            }

            var result = await _userController.UpdateUser(UpdatedUser);
            if (result is NotFoundResult)
            {
                _logger.LogWarning("找不到用户。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "找不到用户";
            }
            //else if (result is ConflictResult)
            //{
            //    _logger.LogWarning("使用ID更新用户时发生并发冲突。UserId: {UserId}", UpdatedUser.Id);
            //    ErrorMessage = "您尝试编辑的记录已被其他用户修改。请重新加载数据，然后重试";

            //}
            else if (result is StatusCodeResult status && status.StatusCode == 500)
            {
                _logger.LogError("更新用户时出错。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "更新用户时出错。";
            }
            else if (result is ObjectResult objResult && objResult.StatusCode == 409)
            {
                _logger.LogWarning("使用ID更新用户时发生并发冲突。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "您尝试编辑的记录已被其他用户修改。请重新加载数据，然后重试";
            }
            else
            {
                _logger.LogInformation("User with ID {UserId} updated successfully", UpdatedUser.Id);
                SuccessMessage = "保存成功";
            }

            List = await _userController.GetTablePage(Limit, PageIndex); // 刷新用户列表
            return Page();
        }
    }
}
