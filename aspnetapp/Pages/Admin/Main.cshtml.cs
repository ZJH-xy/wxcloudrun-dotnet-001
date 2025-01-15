using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin {
	[Authorize] // 确保需要认证才能访问
	[Authorize(Roles = "admin")]// 只有管理员能访问
	public class homeModel : PageModel {
		public IActionResult OnGet() {

			return Page();
		}
	}
}
