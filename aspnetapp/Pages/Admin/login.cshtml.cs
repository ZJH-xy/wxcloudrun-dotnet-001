using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin
{
    public class LoginModel : PageModel {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet() {
            // 处理 GET 请求
        }

        public IActionResult OnPost() {
            // 处理 POST 请求
            if (Username == "admin" && Password == "password") // 示例验证
            {
                // 登录成功，跳转到主页或其他页面
                return RedirectToPage("/Index");
            } else {
                // 登录失败，显示错误信息
                ErrorMessage = "用户名或密码错误";
                return Page();
            }
        }
    }
}