using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetapp.Pages.Admin.Subpages.Overview {
	[Authorize] // 确保需要认证才能访问
	[Authorize(Roles = "admin")]// 只有管理员能访问
	public class StatisticsDataModel : PageModel {
		private readonly StatisticsDataControllerWeb _statisticsDataController;
		private readonly ILogger<StatisticsDataModel> _logger;

		public StatisticsDataModel(StatisticsDataControllerWeb statisticsDataController, ILogger<StatisticsDataModel> logger) {
			_statisticsDataController = statisticsDataController;
			_logger = logger;
		}

		/// <summary>
		/// 用户总数
		/// </summary>
		public int UserSum { get; set; }

		/// <summary>
		/// 车辆总数
		/// </summary>
		public int VehicleSum { get; set; }

		/// <summary>
		/// 门店总数
		/// </summary>
		public int StoreSum { get; set; }

		/// <summary>
		/// 订单总数
		/// </summary>
		public int OrderSum { get; set; }

		/// <summary>
		/// 统计营业额趋势
		/// </summary>
		public List<(string Month, decimal TotalRevenue)> RevenueTrends { get; set; }

		/// <summary>
		/// 最近任意个月的订单趋势
		/// </summary>
		public List<(string Month, int OrderCount)> OrderTrends { get; set; }

		/// <summary>
		/// 最近任意个月的新增用户趋势
		/// </summary>
		public List<(string Month, int UserCount)> UserTrends { get; set; }

		/// <summary>
		/// 车辆状态分布(饼图)
		/// </summary>
		public List<(string State, int VehicleCount)> VehicleStateDistribution { get; set; }

		public async Task<IActionResult> OnGetAsync() {


            UserSum = await _statisticsDataController.GetUserSumAsync();
			VehicleSum = await _statisticsDataController.GetVehicleSumAsync();
			StoreSum = await _statisticsDataController.GetStoreSumAsync();
			OrderSum = await _statisticsDataController.GetOrderSumAsync();

			//OrderTrends = await _statisticsDataController.GetOrderTrendsAsync(6);
			//UserTrends = await _statisticsDataController.GetUserTrendsAsync(6);

			//VehicleStateDistribution = await _statisticsDataController.GetVehicleStateDistributionAsync();
			//var s = await _statisticsDataController.GetRevenueTrendsAsync(6);

			return Page();
		}


		public async Task<IActionResult> OnGetOrderTrendsAsync() {
			try {
				OrderTrends = await _statisticsDataController.GetOrderTrendsAsync(6);

			} catch (Exception e) {

				return new JsonResult(new { success = false, e.Message });
			}

			return new JsonResult(new { success = true, orderTrends = OrderTrends.ToJson() });
		}

		public async Task<IActionResult> OnGetUserTrendsAsync() {
			try {
				UserTrends = await _statisticsDataController.GetUserTrendsAsync(6);

			} catch (Exception e) {

				return new JsonResult(new { success = false, e.Message });
			}
			return new JsonResult(new { success = true, UserTrends = UserTrends.ToJson() });
		}

		public async Task<IActionResult> OnGetVehicleStateDistributionAsync() {
			try {
				VehicleStateDistribution = await _statisticsDataController.GetVehicleStateDistributionAsync();

			} catch (Exception e) {

				return new JsonResult(new { success = false, e.Message });
			}
			return new JsonResult(new { success = true, VehicleStateDistribution = VehicleStateDistribution.ToJson() });
		}

		public async Task<IActionResult> OnGetRevenueTrendsAsync() {
			try {
				RevenueTrends = await _statisticsDataController.GetRevenueTrendsAsync(6);

			} catch (Exception e) {

				return new JsonResult(new { success = false, e.Message });
			}
			return new JsonResult(new { success = true, RevenueTrends = RevenueTrends.ToJson() });
		}
	}
}
