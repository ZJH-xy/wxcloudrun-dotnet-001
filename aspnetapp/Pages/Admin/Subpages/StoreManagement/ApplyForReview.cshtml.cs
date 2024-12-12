using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace aspnetapp.Pages.Admin.Subpages.StoreManagement {
	public class ApplyForReviewModel : PageModel {
		private readonly ILogger<ApplyForReviewModel> _logger;
		private readonly MyDbContext _context;

		public ApplyForReviewModel(MyDbContext context, ILogger<ApplyForReviewModel> logger) {
			_context = context;
			_logger = logger;
		}

		public List<ReviewMerchantAddress> List { get; set; } = new();

		[BindProperty]
		public Vehicle NewVehicle { get; set; } = new();

		// 用于在页面显示错误信息
		public string ErrorMessage { get; set; }

		// 用于在页面显示成功信息
		public string SuccessMessage { get; set; }

		/// <summary>
		/// 默认页码查询
		/// </summary>
		public async Task<IActionResult> OnGetAsync() {
			List = await _context.ReviewMerchantAddress.ToListAsync();

			return Page();
		}

		/// <summary>
		/// 同意
		/// </summary>
		public async Task<IActionResult> OnPostConfirmAsync([FromBody] Dictionary<string, int> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			int id = requestData["id"];

			ReviewMerchantAddress? review = await _context.ReviewMerchantAddress.FindAsync(id);
			if (review is null) {
				ErrorMessage = "未找到申请";
				return new JsonResult(new { success = false, message = "未找到申请" });
			}

			Store? store = await _context.Store.FindAsync(review.TheStore);
			if (store is null)
				return new JsonResult(new { success = false, message = "未找到门店" });

			using var transaction = await _context.Database.BeginTransactionAsync();// 事务开始

			store.Address = review.Address is null ? store.Address : review.Address;
			store.Name = review.Name is null ? store.Name : review.Name;
			store.GpsLongitude = review.GpsLongitude;
			store.GpsLatitude = review.GpsLatitude;

			try {
				_context.Store.Update(store);
				_context.ReviewMerchantAddress.Remove(review);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

			} catch (Exception e) {
				await transaction.RollbackAsync();
				ErrorMessage = "失败";
				return new JsonResult(new { success = false, message = $"失败：{e}" });
			}

			SuccessMessage = "已同意";
			return new JsonResult(new { success = true, message = "已同意" });
		}

		/// <summary>
		/// 拒绝
		/// </summary>
		public async Task<IActionResult> OnPostRefuseAsync([FromBody] Dictionary<string, int> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			int id = requestData["id"];

			ReviewMerchantAddress? review = await _context.ReviewMerchantAddress.FindAsync(id);

			if (review is null) {
				ErrorMessage = "未找到申请";
				return new JsonResult(new { success = false, message = "未找到申请" });
			}

			try {
				_context.ReviewMerchantAddress.Remove(review);
				await _context.SaveChangesAsync();

			} catch (Exception e) {
				ErrorMessage = "失败";
				return new JsonResult(new { success = false, message = $"失败：{e}" });
			}

			SuccessMessage = "已拒绝";
			return new JsonResult(new { success = true, message = "已拒绝" });
		}		
	}
}
