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
		/// 导出功能
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnPostExportToExcelAsync([FromBody] Dictionary<string, string> requestData) {
			// 解析月份字符串
			string monthString = requestData["Month"];
			if (!DateTime.TryParse(monthString, out DateTime targetMonth)) {
				return BadRequest("无效的月份格式");
			}

			// 获取该月份的第一天和最后一天
			DateTime startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
			DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

			// 获取换车记录表
			List<VehicleReplacementRecord> excelList = await _vehicleReplacementRecordControllerWeb.GetAllReplacementRecordsByMonth(startOfMonth, endOfMonth);

			// 创建一个新的工作簿
			IWorkbook workbook = new XSSFWorkbook();
			ISheet sheet = workbook.CreateSheet("换车记录数据");

			// 创建表头行
			IRow headerRow = sheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("序号");
			headerRow.CreateCell(1).SetCellValue("换车记录 ID");

			// 填充数据
			for (int i = 0; i < excelList.Count; i++) {
				var row = sheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(i + 1);
				row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
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
				var fileName = "换车记录数据.xlsx";
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
