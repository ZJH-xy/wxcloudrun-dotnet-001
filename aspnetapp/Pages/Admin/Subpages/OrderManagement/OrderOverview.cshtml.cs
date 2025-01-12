using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using SixLabors.ImageSharp;
using System.Threading.Tasks;

namespace aspnetapp.Pages.Admin.Subpages.OrderManagement {
	public class OrderOverviewModel : PageModel {
		private readonly OrderControllerWeb _orderController;
		private readonly ILogger<OrderOverviewModel> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public OrderOverviewModel(OrderControllerWeb orderController, ILogger<OrderOverviewModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
			_orderController = orderController;
			_logger = logger;
            _wxSetting = wxSetting;
        }

		public List<Order> List { get; set; } = new List<Order>();

		// 用于在页面显示错误信息
		public string ErrorMessage { get; set; }

		// 用于在页面显示成功信息
		public string SuccessMessage { get; set; }

		// 分页查询
		[BindProperty(SupportsGet = true)]
		public int Limit { get; set; } = 10;

		[BindProperty(SupportsGet = true)]
		public static int PageIndex { get; set; } = 1;
		public int PageIndexHtml { get; set; }

		// 更新
		[BindProperty]
		public Order UpdatedOrder { get; set; } = new Order();

		// 查询
		public static List<Order> SearchList { get; set; } = new List<Order>(); // 查询用List

		[BindProperty(SupportsGet = true)]
		public int SearchSum { get; set; } = 0; // 查询结果总数

		// 数据
		[BindProperty(SupportsGet = true)]
		public static string? SearchUserPhone { get; set; }
		public string? SearchUserPhoneHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public static string? SearchStatus { get; set; }
		public string? SearchStatusHtml { get; set; }

		/// <summary>
		/// 车辆
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchTheVehicle { get; set; }
		public string? SearchTheVehicleHtml { get; set; }

		/// <summary>
		/// 租车点
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchTheRentalLocation { get; set; }
		public string? SearchTheRentalLocationHtml { get; set; }

		/// <summary>
		/// 租车点名称
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchStoreName { get; set; }
		public string? SearchStoreNameHtml { get; set; }

		// 排序
		[BindProperty(SupportsGet = true)]
		public static string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"
		public string? SortFieldHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public static string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序
		public string? SortOrderHtml { get; set; }

        // 图片上传
        public string ImageName { get; set; } // 图片文件名
        public static string FileId { get; set; } = "";

        /// <summary>
        /// 默认页码查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync() {
			SearchUserPhone = "";
			SearchStatus = "";
			SearchTheVehicle = null;
			SearchTheRentalLocation = null;
			SearchStoreName = null;
			SortField = "Id";
			SortOrder = "asc";

			try {
				List = await _orderController.GetTablePage(Limit, PageIndex);
			} catch (Exception ex) {
				_logger.LogError(ex, "获取订单列表时出错");
				ModelState.AddModelError(string.Empty, "加载订单列表时发生错误。");
			}
			return Page();
		}

		/// <summary>
		/// 搜索
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostSearchAsync() {

			// 调用 OrderControllerWeb 中的 SearchOrders 方法，包含排序字段和顺序
			(List, SearchSum) = await _orderController.SearchOrders(Limit, PageIndex, SearchUserPhone, SearchStatus, SearchTheVehicle, SearchTheRentalLocation, SearchStoreName, SortField, SortOrder);
			
			SuccessMessage = $"搜索成功，共{SearchSum}条数据";

			return Page();
		}

		/// <summary>
		/// 提交更新订单的操作
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostUpdateOrderAsync() {
			var result = await _orderController.UpdateOrder(UpdatedOrder);
			if (result is NotFoundResult) {
				ErrorMessage = $"未找到订单，订单ID：{UpdatedOrder.Id}";
			} else if (result is StatusCodeResult status && status.StatusCode == 500) {
				_logger.LogError("更新订单时发生错误，订单ID：{OrderId}", UpdatedOrder.Id);
				ErrorMessage = "更新订单时发生错误。";
			} else if (result is ObjectResult objResult && objResult.StatusCode == 403) {
				_logger.LogWarning("订单状态非法，无法更新费用，订单ID：{OrderId}", UpdatedOrder.Id);
				ErrorMessage = $"订单状态非法，费用修改失败，请检查订单状态后重试。订单ID：{UpdatedOrder.Id}";
			} else if (result is ObjectResult objResultConflict && objResultConflict.StatusCode == 409) {
				_logger.LogWarning("更新订单时发生并发冲突，订单ID：{OrderId}", UpdatedOrder.Id);
				ErrorMessage = $"您尝试编辑的记录已被其他用户修改，请重新加载数据后重试。订单ID：{UpdatedOrder.Id}";
			} else {
				_logger.LogInformation("订单更新成功，订单ID：{OrderId}", UpdatedOrder.Id);
				// 计算行号
				var rowIndex = List.FindIndex(order => order.Id == UpdatedOrder.Id) + 1; // 行号从1开始
				SuccessMessage = $"保存成功，订单已更新，订单ID：{UpdatedOrder.Id}";
			}

			List = await _orderController.GetTablePage(Limit, PageIndex); // 刷新订单列表

			return Page();
		}

		/// <summary>
		/// 导出功能
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnPostExportToExcelAsync([FromBody] Dictionary<string, string> requestData) {
			// 解析月份字符串
			string monthString = requestData["Month"];
			if (!DateTime.TryParse(monthString, out DateTime targetMonth)) {
				ErrorMessage = "无效的月份格式";
				return BadRequest("无效的月份格式");
			}

			// 获取该月份的第一天和最后一天
			DateTime startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
			DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

			// 获取订单表
			List<Order> excelList = await _orderController.GetAllOrdersByMonth(startOfMonth, endOfMonth);

			// 创建一个新的工作簿
			IWorkbook workbook = new XSSFWorkbook();
			ISheet sheet = workbook.CreateSheet("订单数据");

			// 创建表头行
			IRow headerRow = sheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("序号");
			headerRow.CreateCell(1).SetCellValue("订单 ID");
			headerRow.CreateCell(2).SetCellValue("商户系统内部订单号");
			headerRow.CreateCell(3).SetCellValue("微信支付系统订单号");
			headerRow.CreateCell(4).SetCellValue("用户手机号");
			headerRow.CreateCell(5).SetCellValue("逻辑指向用户");
			headerRow.CreateCell(6).SetCellValue("逻辑指向套餐");
			headerRow.CreateCell(7).SetCellValue("实际起始时间");
			headerRow.CreateCell(8).SetCellValue("实际归还时间");
			headerRow.CreateCell(9).SetCellValue("逻辑指向车辆（租用车辆）");
			headerRow.CreateCell(10).SetCellValue("租车点（StoreId）");
			headerRow.CreateCell(11).SetCellValue("还车点（StoreId）");
			headerRow.CreateCell(12).SetCellValue("用户姓名");
			headerRow.CreateCell(13).SetCellValue("身份证号");
			headerRow.CreateCell(14).SetCellValue("押金");
			headerRow.CreateCell(15).SetCellValue("租金");
			headerRow.CreateCell(16).SetCellValue("调度费");
			headerRow.CreateCell(17).SetCellValue("超时费");
			headerRow.CreateCell(18).SetCellValue("其他费用");
			headerRow.CreateCell(19).SetCellValue("已付");
			headerRow.CreateCell(20).SetCellValue("已退押金");
			headerRow.CreateCell(21).SetCellValue("订单状态");
			headerRow.CreateCell(22).SetCellValue("备注");
			headerRow.CreateCell(23).SetCellValue("支付完成时间");
			headerRow.CreateCell(24).SetCellValue("创建时间");
			headerRow.CreateCell(25).SetCellValue("更新时间");

			// 填充数据
			for (int i = 0; i < excelList.Count; i++) {
				var row = sheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(i + 1);
				row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
				row.CreateCell(2).SetCellValue(excelList[i].OutTradeNo ?? "");
				row.CreateCell(3).SetCellValue(excelList[i].TransactionId ?? "");
				row.CreateCell(4).SetCellValue(excelList[i].UserPhone ?? "");
				row.CreateCell(5).SetCellValue(excelList[i].TheUser.ToString());
				row.CreateCell(6).SetCellValue(excelList[i].TheStoreMenu.ToString());
				row.CreateCell(7).SetCellValue(excelList[i].ActualStartingTime.HasValue ? excelList[i].ActualStartingTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
				row.CreateCell(8).SetCellValue(excelList[i].ActualReturnTime.HasValue ? excelList[i].ActualReturnTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
				row.CreateCell(9).SetCellValue(excelList[i].TheVehicle.ToString());
				row.CreateCell(10).SetCellValue(excelList[i].TheRentalLocation.ToString());
				row.CreateCell(11).SetCellValue(excelList[i].TheReturnThePoint.HasValue ? excelList[i].TheReturnThePoint.Value.ToString() : "");
				row.CreateCell(12).SetCellValue(excelList[i].UserName ?? "");
				row.CreateCell(13).SetCellValue(excelList[i].IdentityCard ?? "");
				row.CreateCell(14).SetCellValue(excelList[i].Deposit.ToString("F2")); // 保留两位小数
				row.CreateCell(15).SetCellValue(excelList[i].Rent.ToString("F2")); // 保留两位小数
				row.CreateCell(16).SetCellValue(excelList[i].DispatchFee.ToString("F2")); // 保留两位小数
				row.CreateCell(17).SetCellValue(excelList[i].OvertimeFee.ToString("F2")); // 保留两位小数
				row.CreateCell(18).SetCellValue(excelList[i].OtherFees.ToString("F2")); // 保留两位小数
				row.CreateCell(19).SetCellValue(excelList[i].Paid.ToString("F2")); // 保留两位小数
				row.CreateCell(20).SetCellValue(excelList[i].DepositRefunded.ToString("F2")); // 保留两位小数
				row.CreateCell(21).SetCellValue(excelList[i].Status.ToString());
				row.CreateCell(22).SetCellValue(excelList[i].Notes ?? "");
				row.CreateCell(23).SetCellValue(excelList[i].SuccessTime.HasValue ? excelList[i].SuccessTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
				row.CreateCell(24).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
				row.CreateCell(25).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
			}

			// 固定列宽
			for (int col = 0; col < 26; col++) {
				sheet.SetColumnWidth(col, 20 * 256); // 设置固定宽度，单位为 1/256 个字符宽度
			}

			// 将工作簿保存到内存流
			using (var memoryStream = new MemoryStream()) {
				workbook.Write(memoryStream);
				var fileName = "订单数据.xlsx";
				var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				// 返回文件流供下载
				return File(memoryStream.ToArray(), contentType, fileName);
			}
		}

		/// <summary>
		/// 获取总页数
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetPageSumAsync() {
			try {
				// 获取所有订单数据的总数
				var sum = await _orderController.GetOrderCount();

				// 计算总页数
				int totalPages = (sum / Limit) + 1;

				// 返回总页数
				return new JsonResult(new { totalPages });
			} catch (Exception ex) {
				_logger.LogError(ex, "获取总页数时发生错误");
				ErrorMessage = "无法获取总页数";

				return BadRequest("无法获取总页数");
			}
		}

		/// <summary>
		/// 获取页码
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetPageIndexAsync() {
			return new JsonResult(new { success = true, pageIndex = PageIndex });
		}

		/// <summary>
		/// 更改页码
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostChangePageAsync([FromBody] Dictionary<string, int> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			PageIndex = requestData["PageIndex"];

			return new JsonResult(new { success = true, message = "成功", pageIndex = PageIndex });
		}

		/// <summary>
		/// 更改查询数据
		/// </summary>
		/// <param name="requestData"></param>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostChangeSearchDataAsync([FromBody] Dictionary<string, string> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			SearchUserPhone = requestData["SearchUserPhone"];
			SearchStatus = requestData["SearchStatus"];
			SearchTheVehicle = requestData["SearchTheVehicle"];
			SearchTheRentalLocation = requestData["SearchTheRentalLocation"];
			SearchStoreName = requestData["SearchStoreName"];
            SortField = requestData["SortField"];
            SortOrder = requestData["SortOrder"];

            return new JsonResult(new { success = true, message = "成功" });
		}

		/// <summary>
		/// 获取查询数据
		/// </summary>
		/// <returns></returns>
		public async Task<JsonResult> OnGetSearchDataAsync() {
			return new JsonResult(new { success = true, message = "成功", SearchUserPhone, SearchStatus, SearchTheVehicle, SearchTheRentalLocation, SearchStoreName, SortField, SortOrder });
		}
	}
}
