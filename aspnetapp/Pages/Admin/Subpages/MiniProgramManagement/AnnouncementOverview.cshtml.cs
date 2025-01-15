using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace aspnetapp.Pages.Admin.Subpages.MiniProgramManagement {
	/// <summary>
	/// 公告表
	/// </summary>
	[Authorize] // 确保需要认证才能访问
	[Authorize(Roles = "admin")]// 只有管理员能访问
	public class AnnouncementOverviewModel : PageModel {
		private readonly NoticeControllerWeb _noticeController;
		private readonly ILogger<AnnouncementOverviewModel> _logger;

		public AnnouncementOverviewModel(NoticeControllerWeb advertisement, ILogger<AnnouncementOverviewModel> logger) {
			_noticeController = advertisement;
			_logger = logger;
		}
		public List<Notice> List { get; set; } = new();

		[BindProperty]
		public Notice NewAdvertisement { get; set; } = new();

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
		public Notice UpdatedNotice { get; set; } = new();

		// 查询
		public static List<Notice> SearchList { get; set; } = new(); // 查询用List

		[BindProperty(SupportsGet = true)]
		public int SearchSum { get; set; } = 0; // 查询结果总数

		// 查询字段
		[BindProperty(SupportsGet = true)]
		public static string? SearchTitle { get; set; }// 标题
		public string? SearchTitleHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? SearchContentHtml { get; set; }// 内容
		public string? SearchContent { get; set; }

		// 排序
		[BindProperty(SupportsGet = true)]
		public static string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"
		public string? SortFieldHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public static string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序
		public string? SortOrderHtml { get; set; }

		/// <summary>
		/// 删除
		/// </summary>
		/// <returns></returns>
		[HttpDelete]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnDeleteDelAsync([FromBody] Dictionary<string, int> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			int id = requestData["id"];
			if (id == 0)
				return new JsonResult(new { success = false, message = "ID不能为0" });


			return new JsonResult(new { success = true, message = $"已删除行数：{await _noticeController.Delete(id)}" });
		}

		/// <summary>
		/// 添加
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostAddAsync() {
			await _noticeController.AddAsync(NewAdvertisement);
			SuccessMessage = $"添加成功";

			List = await _noticeController.GetTablePage(Limit, PageIndex); // 刷新列表

			return Page();
		}

		/// <summary>
		/// 默认页码查询
		/// </summary>
		public async Task<IActionResult> OnGetAsync() {

			SearchTitle = null;
			SearchContent = null;
			SortField = "Id";
			SortOrder = "asc";

			try {
				List = await _noticeController.GetTablePage(Limit, PageIndex);
			} catch (Exception ex) {
				_logger.LogError(ex, "获取车辆列表时出错");
				ModelState.AddModelError(string.Empty, "加载车辆列表时发生错误。");
			}

			return Page();
		}

		/// <summary>
		/// 搜索
		/// </summary>
		public async Task<IActionResult> OnPostSearchAsync() {
			(List, SearchSum) = await _noticeController.SearchAsync(Limit, PageIndex, SearchTitle, SearchContent, SortField, SortOrder);

			SuccessMessage = $"搜索成功，共{SearchSum}条数据";


			return Page();
		}

		/// <summary>
		/// 更新
		/// </summary>
		public async Task<IActionResult> OnPostUpdateAsync() {
			var result = await _noticeController.UpdateAsync(UpdatedNotice);
			if (result is NotFoundResult) {
				_logger.LogWarning("找不到车辆。VehicleId: {VehicleId}", UpdatedNotice.Id);
				ErrorMessage = "找不到车辆";
			} else if (result is StatusCodeResult status && status.StatusCode == 500) {
				_logger.LogError("更新车辆时出错。VehicleId: {VehicleId}", UpdatedNotice.Id);
				ErrorMessage = "更新车辆时出错。";
			} else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
				_logger.LogWarning("使用ID更新车辆时发生并发冲突。VehicleId: {VehicleId}", UpdatedNotice.Id);
				ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedNotice.Id}";
			} else {
				_logger.LogInformation("ID为{VehicleId}的车辆已成功更新", UpdatedNotice.Id);
				SuccessMessage = $"保存成功，已更新 ID：{UpdatedNotice.Id}";
			}

			List = await _noticeController.GetTablePage(Limit, PageIndex); // 刷新车辆列表

			Thread.Sleep(1500);

			return Page();
		}

		/// <summary>
		/// 返回总页数
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetPageSumAsync() {
			try {
				// 获取所有用户数据的总数（可以通过服务方法获取）
				var sum = await _noticeController.GetPageSum();

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
		public async Task<IActionResult> OnGetPageIndexAsync() {
			return new JsonResult(new { success = true, pageIndex = PageIndex });
		}

		/// <summary>
		/// 更改页码
		/// </summary>
		public async Task<JsonResult> OnPostChangePageAsync([FromBody] Dictionary<string, int> requestData) {
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			PageIndex = requestData["PageIndex"];

			return new JsonResult(new { success = true, message = "成功", pageIndex = PageIndex });
		}

		/// <summary>
		/// 导出功能
		/// </summary>
		public async Task<IActionResult> OnGetExportToExcelAsync() {
			List<Notice> excelList = await _noticeController.GetAllListAsync();

			// 创建一个新的工作簿
			IWorkbook workbook = new XSSFWorkbook();
			ISheet sheet = workbook.CreateSheet("公告数据");

			// 创建表头行
			IRow headerRow = sheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("序号");
			headerRow.CreateCell(1).SetCellValue("公告 ID");
			headerRow.CreateCell(2).SetCellValue("标题");
			headerRow.CreateCell(3).SetCellValue("内容");
			headerRow.CreateCell(4).SetCellValue("创建时间");
			headerRow.CreateCell(5).SetCellValue("更新时间");

			// 填充数据
			for (int i = 0; i < excelList.Count; i++) {
				var row = sheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(i + 1);
				row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
				row.CreateCell(2).SetCellValue(excelList[i].Title ?? "");
				row.CreateCell(3).SetCellValue(excelList[i].Content ?? "");
				row.CreateCell(4).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
				row.CreateCell(5).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
			}

			// 固定列宽
			for (int col = 0; col < 6; col++) {
				sheet.SetColumnWidth(col, 20 * 256); // 设置固定宽度，单位为 1/256 个字符宽度
			}

			// 将工作簿保存到内存流
			using (var memoryStream = new MemoryStream()) {
				workbook.Write(memoryStream);
				var fileName = "公告数据.xlsx";
				var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				// 返回文件流供下载
				return File(memoryStream.ToArray(), contentType, fileName);
			}
		}

		/// <summary>
		/// 获取查询数据
		/// </summary>
		public async Task<JsonResult> OnGetSearchDataAsync() {
			return new JsonResult(new { success = true, message = "成功", SearchTitle, SearchContent, SortField, SortOrder });
		}

		/// <summary>
		/// 更改查询数据
		/// </summary>
		public async Task<JsonResult> OnPostChangeSearchDataAsync([FromBody] Dictionary<string, string> requestData) {
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			SearchTitle = requestData["SearchTitle"];
			SearchContent = requestData["SearchContent"];
			SortField = requestData["SortField"];
			SortOrder = requestData["SortOrder"];

			return new JsonResult(new { success = true, message = "成功" });
		}
	}
}
