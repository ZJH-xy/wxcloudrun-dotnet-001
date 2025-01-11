using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace aspnetapp.Pages.Admin.Subpages.OrderManagement {
	public class VehicleReplacementRecordModel : PageModel {
		private readonly VehicleReplacementRecordControllerWeb _vehicleReplacementRecordControllerWeb;
		private readonly ILogger<VehicleReplacementRecordModel> _logger;

		public VehicleReplacementRecordModel(VehicleReplacementRecordControllerWeb orderController, ILogger<VehicleReplacementRecordModel> logger) {
			_vehicleReplacementRecordControllerWeb = orderController;
			_logger = logger;
		}

		public List<VehicleReplacementRecord> List { get; set; } = new List<VehicleReplacementRecord>();

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
		public VehicleReplacementRecord UpdatedOrder { get; set; } = new VehicleReplacementRecord();

		// 查询
		public static List<VehicleReplacementRecord> SearchList { get; set; } = new List<VehicleReplacementRecord>(); // 查询用List

		[BindProperty(SupportsGet = true)]
		public int SearchSum { get; set; } = 0; // 查询结果总数

		// 数据
		/// <summary>
		/// 换车记录ID
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchTheOrder { get; set; }
		public string? SearchTheOrderHtml { get; set; }

		/// <summary>
		/// 状态
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchStatus { get; set; }
		public string? SearchStatusHtml { get; set; }

		/// <summary>
		/// 车辆
		/// </summary>
		[BindProperty(SupportsGet = true)]
		public static string? SearchTheStore { get; set; }
		public string? SearchTheStoreHtml { get; set; }

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
			SearchTheOrder = null;
			SearchStatus = null;
			SearchTheStore = null;
			SortField = "Id";
			SortOrder = "asc";

			try {
				List = await _vehicleReplacementRecordControllerWeb.GetTablePage(Limit, PageIndex);
			} catch (Exception ex) {
				_logger.LogError(ex, "获取换车记录列表时出错");
				ModelState.AddModelError(string.Empty, "加载换车记录列表时发生错误。");
			}
			return Page();
		}

		/// <summary>
		/// 搜索
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostSearchAsync() {

			// 调用 OrderControllerWeb 中的 SearchOrders 方法，包含排序字段和顺序
			(List, SearchSum) = await _vehicleReplacementRecordControllerWeb.SearchReplacementRecords(Limit, PageIndex, SearchTheOrder, SearchStatus, SearchTheStore, SortField, SortOrder);

			return Page();
		}

		/// <summary>
		/// 导出换车记录到 Excel
		/// </summary>
		/// <returns>Excel 文件</returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnPostExportToExcelAsync([FromBody] Dictionary<string, string> requestData) {
			try {
				// 解析月份字符串
				if (!requestData.TryGetValue("Month", out string monthString) ||
					!DateTime.TryParse(monthString, out DateTime targetMonth)) {
					return BadRequest("无效的月份格式");
				}

				// 获取该月份的第一天和最后一天
				DateTime startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
				DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

				// 获取换车记录数据
				List<VehicleReplacementRecord> excelList = await _vehicleReplacementRecordControllerWeb.GetAllReplacementRecordsByMonth(startOfMonth, endOfMonth);

				// 创建一个新的工作簿
				IWorkbook workbook = new XSSFWorkbook();
				ISheet sheet = workbook.CreateSheet("换车记录数据");

				// 表头定义
				var headers = new[] {
					"序号", "换车记录 ID", "订单编号", "旧车辆 ID", "新车辆 ID", "换车门店 ID",
					"状态", "创建时间", "更新时间"
				};

				// 创建表头行
				IRow headerRow = sheet.CreateRow(0);
				for (int col = 0; col < headers.Length; col++) {
					headerRow.CreateCell(col).SetCellValue(headers[col]);
				}

				// 填充数据
				for (int i = 0; i < excelList.Count; i++) {
					var record = excelList[i];
					var row = sheet.CreateRow(i + 1);

					row.CreateCell(0).SetCellValue(i + 1); // 序号
					row.CreateCell(1).SetCellValue(record.Id); // 换车记录 ID
					row.CreateCell(2).SetCellValue(record.TheOrder); // 订单编号
					row.CreateCell(3).SetCellValue(record.TheOldVehicles); // 旧车辆 ID
					row.CreateCell(4).SetCellValue(record.TheNewVehicles); // 新车辆 ID
					row.CreateCell(5).SetCellValue(record.TheStore); // 换车门店 ID
					row.CreateCell(6).SetCellValue(record.State.ToString()); // 状态
					row.CreateCell(7).SetCellValue(record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")); // 创建时间
					row.CreateCell(8).SetCellValue(record.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")); // 更新时间
				}

				// 设置列宽
				for (int col = 0; col < headers.Length; col++) {
					sheet.AutoSizeColumn(col); // 根据内容自动调整列宽
				}

				// 将工作簿保存到内存流
				using var memoryStream = new MemoryStream();
				workbook.Write(memoryStream);
				var fileName = $"换车记录数据_{targetMonth:yyyyMM}.xlsx";
				var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				// 返回文件流供下载
				return File(memoryStream.ToArray(), contentType, fileName);
			} catch (Exception ex) {
				// 捕获异常并返回错误信息
				return StatusCode(500, $"导出失败：{ex.Message}");
			}
		}

		/// <summary>
		/// 获取总页数
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetPageSumAsync() {
			try {
				// 获取所有换车记录数据的总数
				var sum = await _vehicleReplacementRecordControllerWeb.GetReplacementRecordCount();

				// 计算总页数
				int totalPages = (sum / Limit) + 1;

				// 返回总页数
				return new JsonResult(new { totalPages });
			} catch (Exception ex) {
				_logger.LogError(ex, "获取总页数时发生错误");
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

			SearchTheOrder = requestData["SearchUserPhone"];
			SearchStatus = requestData["SearchStatus"];
			SearchTheStore = requestData["SearchTheVehicle"];
			SortField = requestData["SortField"];
			SortOrder = requestData["SortOrder"];

			return new JsonResult(new { success = true, message = "成功" });
		}

		/// <summary>
		/// 获取查询数据
		/// </summary>
		/// <returns></returns>
		public async Task<JsonResult> OnGetSearchDataAsync() {
			return new JsonResult(new { success = true, message = "成功", SearchTheOrder, SearchStatus, SearchTheStore, SortField, SortOrder });
		}
	}
}
