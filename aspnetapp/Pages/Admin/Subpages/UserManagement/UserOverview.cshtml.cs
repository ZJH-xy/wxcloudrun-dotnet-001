using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;
        private readonly ILogger<UserOverviewModel> _logger;

        public List<User> List { get; set; } = new List<User>();
        public class NewUser
        {
            public string Nickname { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public string IdentityCard { get; set; }
            public IFormFile Image { get; set; } // 用于接收文件
        }


        // 用于在页面显示错误信息
        public string ErrorMessage { get; set; }

        // 用于在页面显示成功信息
        public string SuccessMessage { get; set; }

        // 分页查询
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        // 更新
        [BindProperty]
        public User UpdatedUser { get; set; } = new User();

        // 查询
        [BindProperty(SupportsGet = true)]
        public string? SearchPhone { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchNickname { get; set; }

        // 排序
        [BindProperty(SupportsGet = true)]
        public string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序


        public UserOverviewModel(UserControllerWeb userController, ILogger<UserOverviewModel> logger) {
            _userController = userController;
            _logger = logger;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync() {
            _logger.LogInformation("正在获取限制为{Limit}的页面{PageIndex}的用户列表", PageIndex, Limit);
            try {
                List = await _userController.GetTablePage(Limit, PageIndex);

            } catch (Exception ex) {
                _logger.LogError(ex, "获取用户列表时出错");
                ErrorMessage = "加载用户列表时发生错误。";
                ModelState.AddModelError(string.Empty, "加载用户列表时发生错误。");
            }
            return Page();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateUserAsync() {
            _logger.LogInformation("正在尝试使用ID更新用户{UserId}", UpdatedUser.Id);

            if (!ModelState.IsValid) {
                _logger.LogWarning("表单验证失败。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "表单验证失败，请检查输入内容";
                return Page();
            }

            var result = await _userController.UpdateUser(UpdatedUser);
            if (result is NotFoundResult) {
                _logger.LogWarning("找不到用户。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "找不到用户";
            }
            else if (result is StatusCodeResult status && status.StatusCode == 500) {
                _logger.LogError("更新用户时出错。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "更新用户时出错。";
            } else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedUser.Id}";
            } else {
                _logger.LogInformation("ID为{UserId}的用户已成功更新", UpdatedUser.Id);
                // 计算行号
                var rowIndex = List.FindIndex(user => user.Id == UpdatedUser.Id) + 1; // 行号从1开始
                SuccessMessage = $"保存成功，已更新 ID：{UpdatedUser.Id}";
            }

            List = await _userController.GetTablePage(Limit, PageIndex); // 刷新用户列表
            return Page();
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync() {
            _logger.LogDebug("Executing OnGetSearchAsync with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                           SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            // 调用 UserControllerWeb 中的 SearchUsers 方法，包含排序字段和顺序
            List = await _userController.SearchUsers(SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            return Page();
        }


        public IActionResult OnGetSearchAsync(string SearchPhone) {
            Console.WriteLine("=========================");
            return Page();
        }
    }
}
