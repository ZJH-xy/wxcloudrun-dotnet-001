using aspnetapp.Controllers.Web;
using aspnetapp.Pages.Admin.Subpages.StoreManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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

		public IActionResult OnGetAsync() {
			return Page();
		}


		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnPostAsync(/*[FromBody] Dictionary<string, string> requestData*/) {
			// 确保接收到的数据被正确绑定
			//if (requestData == null || !requestData.Any()) {
			//	return new JsonResult(new { success = false, message = "请求数据为空！" });
			//}

			//Username = requestData["Username"];
			//Password = requestData["Password"];

			// 处理 POST 请求
			var re = await _loginControllerWeb.LoginAsync(Username, Password);

			if (re.Item1) {
                // 设置 JWT 到 Cookie 中
                Response.Cookies.Append("JWT", re.Item2, new CookieOptions
                {
                    HttpOnly = true, // 确保 JWT 只能通过服务器端访问，增强安全性
                    Secure = true,   // 仅在 HTTPS 下传输
                    SameSite = SameSiteMode.Strict, // 限制跨站请求
                    Expires = DateTimeOffset.UtcNow.AddHours(1) // JWT 过期时间
                });
                return RedirectToPage("/admin/main");

				// 登录成功
				//return StatusCode(200, new{ JWT = re.Item2 });

				// 跳转到主页或其他页面
				//return RedirectToPage("/admin/main", new { JWT = re.Item2 });
			} else {
				// 登录失败，显示错误信息
				ErrorMessage = "用户名或密码错误";
				return Page();
			}
		}

		/// <summary>
		/// JWT 获取用户id
		/// </summary>
		/// <returns></returns>
		public int GetUserIdInt() {
			return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
		}

		// JWT 获取用户id
		public string GetUserIdString() {
			return User.FindFirstValue(ClaimTypes.NameIdentifier);
		}
	}
}