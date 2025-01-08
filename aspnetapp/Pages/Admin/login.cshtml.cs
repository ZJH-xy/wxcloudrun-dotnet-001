using aspnetapp.Controllers.Web;
using aspnetapp.Pages.Admin.Subpages.StoreManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin {
	public class LoginModel : PageModel {
		private readonly ILogger<LoginModel> _logger;
		private readonly LoginControllerWeb _loginControllerWeb;

		public LoginModel(LoginControllerWeb loginControllerWeb, ILogger<LoginModel> logger) {
			_loginControllerWeb = loginControllerWeb;
			_logger = logger;
		}

		[BindProperty]
		public string Username { get; set; }

		[BindProperty]
		public string Password { get; set; }

		public string ErrorMessage { get; set; }

		public IActionResult OnGet() {
			return Page();
		}

		public IActionResult OnPost() {
			// 处理 POST 请求
			if (_loginControllerWeb.Login(Username, Password)) {
				// 登录成功，跳转到主页或其他页面
				return RedirectToPage("/admin/main");
			} else {
				// 登录失败，显示错误信息
				ErrorMessage = "用户名或密码错误";
				return Page();
			}
		}
	}
}