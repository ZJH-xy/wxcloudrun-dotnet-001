using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using SixLabors.ImageSharp;

namespace aspnetapp.Pages.Admin.Subpages.MiniProgramManagement {
	/// <summary>
	/// 广告
	/// </summary>
	public class AdvertisingOverviewModel : PageModel {
		private readonly AdvertisementControllerWeb _advertisementController;
		private readonly ILogger<AdvertisingOverviewModel> _logger;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public AdvertisingOverviewModel(AdvertisementControllerWeb vehicleController, ILogger<AdvertisingOverviewModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
			_advertisementController = vehicleController;
			_logger = logger;
			_wxSetting = wxSetting;
		}

		public List<Advertisement> List { get; set; } = new();

		[BindProperty]
		public Advertisement NewAdvertisement { get; set; } = new();

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
		public Advertisement UpdatedAdvertisement { get; set; } = new();

		// 查询
		public static List<Advertisement> SearchList { get; set; } = new(); // 查询用List

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

		// 图片上传
		public string ImageName { get; set; } // 图片文件名
		public static string FileId { get; set; } = "";

		/// <summary>
		/// 上传图片
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostSayHelloAsync() {
			Console.WriteLine("接口已触发");
			string message = "未定义错误"; // 默认错误信息
			try {
				var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
				var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
				var envId = _wxSetting.Value.Env;

				// 模拟业务逻辑
				ImageName = "admin/advertisement/" + Guid.NewGuid().ToString() + ".jpg";

				WxUploadFileJsonResult result = TcbApi.UploadFile(appId, envId, ImageName);

				if (result.ErrorCodeValue != 0) {
					message = "上传失败，错误码：" + result.ErrorCodeValue;
					Console.WriteLine(message);
					return new JsonResult(new { success = false, message });
				}

				FileId = result.file_id;
#if DEBUG
				Console.WriteLine($"file_id更新：{FileId}");
#endif

				string filePath = ImageName;
				return new JsonResult(new {
					success = true,
					ImageUrl = result.url,
					FilePath = filePath,
					Authorization = result.authorization,
					Token = result.token,
					Cos_file_id = result.cos_file_id
				});
			} catch (Exception ex) {
				message = "发生异常：" + ex.Message;
				Console.WriteLine(message);
				return new JsonResult(new { success = false, message });
			}
		}

		/// <summary>
		/// 获取文件下载链接
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostImageDownloadAsync([FromBody] Dictionary<string, string> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
			var envId = _wxSetting.Value.Env;

			List<FileItem> fileid_list = new() {
				new FileItem {
					fileid = requestData["fileid"],
					max_age = 7200
				}
				};

			var re = await TcbApi.BatchDownloadFileAsync(appId, envId, fileid_list);
			if (re.errcode != ReturnCode.请求成功) {
				_logger.LogError("{errmsg},获取下载链接{fileid_list}", re.errmsg, fileid_list.ToJson());
			}

			return new JsonResult(new { success = true, message = "获取文件下载链接结束", re.file_list });
		}

		/// <summary>
		/// 添加
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostAddAsync() {
			await _advertisementController.AddAsync(NewAdvertisement);
			SuccessMessage = $"添加成功";

			List = await _advertisementController.GetTablePageAsync(Limit, PageIndex); // 刷新列表

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
				List = await _advertisementController.GetTablePageAsync(Limit, PageIndex);
			} catch (Exception ex) {
				_logger.LogError(ex, "获取广告列表时出错");
				ModelState.AddModelError(string.Empty, "加载广告列表时发生错误。");
			}

			return Page();
		}

		/// <summary>
		/// 搜索
		/// </summary>
		public async Task<IActionResult> OnPostSearchAsync() {
			(List, SearchSum) = await _advertisementController.SearchAsync(Limit, PageIndex, SearchTitle, SearchContent, SortField, SortOrder);
			//SuccessMessage = "搜索成功";

			return Page();
		}

		/// <summary>
		/// 更新
		/// </summary>
		public async Task<IActionResult> OnPostUpdateAsync() {
			var result = await _advertisementController.UpdateAsync(UpdatedAdvertisement);
			if (result is NotFoundResult) {
				_logger.LogWarning("找不到广告。VehicleId: {VehicleId}", UpdatedAdvertisement.Id);
				ErrorMessage = "找不到广告";
			} else if (result is StatusCodeResult status && status.StatusCode == 500) {
				_logger.LogError("更新广告时出错。VehicleId: {VehicleId}", UpdatedAdvertisement.Id);
				ErrorMessage = "更新广告时出错。";
			} else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
				_logger.LogWarning("使用ID更新广告时发生并发冲突。VehicleId: {VehicleId}", UpdatedAdvertisement.Id);
				ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedAdvertisement.Id}";
			} else {
				_logger.LogInformation("ID为{VehicleId}的广告已成功更新", UpdatedAdvertisement.Id);
				SuccessMessage = $"保存成功，已更新 ID：{UpdatedAdvertisement.Id}";
			}

			List = await _advertisementController.GetTablePageAsync(Limit, PageIndex); // 刷新广告列表

			Thread.Sleep(2500);

			if (!FileId.IsNullOrEmpty()) {
#if DEBUG
				_logger.LogInformation($"更新车辆图片信息FileId：{FileId}");
#endif
				// 更新车辆图片信息
				await _advertisementController.PutImagePath(UpdatedAdvertisement.Id, FileId);
				FileId = "";
			}

			return Page();
		}

		/// <summary>
		/// 返回总页数
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetPageSumAsync() {
			try {
				// 获取所有用户数据的总数（可以通过服务方法获取）
				var sum = await _advertisementController.GetPageSum();

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
			List<Advertisement> excelList = await _advertisementController.GetAllListAsync();

			// 创建一个新的工作簿
			IWorkbook workbook = new XSSFWorkbook();
			ISheet sheet = workbook.CreateSheet("广告数据");

			// 创建表头行
			IRow headerRow = sheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("序号");
			headerRow.CreateCell(1).SetCellValue("广告 ID");
			headerRow.CreateCell(2).SetCellValue("标题");
			headerRow.CreateCell(3).SetCellValue("内容");
			headerRow.CreateCell(4).SetCellValue("跳转链接");
			headerRow.CreateCell(5).SetCellValue("创建时间");
			headerRow.CreateCell(6).SetCellValue("更新时间");

			// 填充数据
			for (int i = 0; i < excelList.Count; i++) {
				var row = sheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(i + 1);
				row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
				row.CreateCell(2).SetCellValue(excelList[i].Title ?? "");
				row.CreateCell(3).SetCellValue(excelList[i].Content ?? "");
				row.CreateCell(4).SetCellValue(excelList[i].Src ?? "");
				row.CreateCell(5).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
				row.CreateCell(6).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
			}

			// 固定列宽
			for (int col = 0; col < 7; col++) {
				sheet.SetColumnWidth(col, 20 * 256); // 设置固定宽度，单位为 1/256 个字符宽度
			}

			// 将工作簿保存到内存流
			using (var memoryStream = new MemoryStream()) {
				workbook.Write(memoryStream);
				var fileName = "广告数据.xlsx";
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
