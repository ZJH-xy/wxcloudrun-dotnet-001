using Microsoft.AspNetCore.Mvc.RazorPages;
using aspnetapp.Controllers.Web;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using aspnetapp.Controllers.Miniprogram;

namespace aspnetapp.Pages.Admin.Subpages.VehicleManagement {
	public class VehicleOverviewModel : PageModel {
		private readonly VehicleControllerWeb _vehicleController;
		private readonly ILogger<VehicleOverviewModel> _logger;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public VehicleOverviewModel(VehicleControllerWeb vehicleController, ILogger<VehicleOverviewModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
			_vehicleController = vehicleController;
			_logger = logger;
			_wxSetting = wxSetting;
		}

		public List<Vehicle> List { get; set; } = new();

        [BindProperty]
        public Vehicle NewVehicle { get; set; } = new();

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
		public Vehicle UpdatedVehicle { get; set; } = new Vehicle();

		// 查询
		public static List<Vehicle> SearchList { get; set; } = new(); // 查询用List

		[BindProperty(SupportsGet = true)]
		public int SearchSum { get; set; } = 0; // 查询结果总数

		// 查询字段
		[BindProperty(SupportsGet = true)]
		public static string? SearchPlateNumber { get; set; }
		public string? SearchPlateNumberHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public static string? SearchOwner { get; set; }
		public string? SearchOwnerHtml { get; set; }

		// 排序
		[BindProperty(SupportsGet = true)]
		public string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"

		[BindProperty(SupportsGet = true)]
		public string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序

		// 图片上传
		public string ImageName { get; set; } // 图片文件名
		public static string FileId { get; set; } = "";

        /// <summary>
        /// 用于提供给前端的 API 方法
        /// </summary>
        /// <param name="requestData"></param>
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
                ImageName = "admin/vehicle/pictures/" + Guid.NewGuid().ToString() + ".jpg";
                Console.WriteLine($"[97]ImageName: {ImageName}");

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
		/// 添加
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> OnPostAddStoreAsync() {
			await _vehicleController.AddVehicleAsync(NewVehicle);
			SuccessMessage = $"添加成功";
			List = await _vehicleController.GetTablePage(Limit, PageIndex); // 刷新列表

			return Page();
		}

		/// <summary>
		/// 默认页码查询
		/// </summary>
		public async Task<IActionResult> OnGetAsync() {
			_logger.LogInformation("[OnGetAsync]正在获取限制为{Limit}的页面{PageIndex}的车辆列表", PageIndex, Limit);

			SearchPlateNumber = "";
			SearchOwner = "";

			try {
				List = await _vehicleController.GetTablePage(Limit, PageIndex);
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
			_logger.LogDebug("[OnPostSearchAsync]正在条件查询: PlateNumber: {PlateNumber}, Owner: {Owner}, SortField: {SortField}, SortOrder: {SortOrder}",
							SearchPlateNumber, SearchOwner, SortField, SortOrder);

			// 调用 VehicleControllerWeb 中的 SearchVehicles 方法，包含排序字段和顺序
			SearchList = await _vehicleController.SearchVehicles(SearchPlateNumber, SearchOwner, SortField, SortOrder);

			List = SearchList
				.Skip((PageIndex - 1) * Limit) // 跳过前面页的数据
				.Take(Limit).ToList(); // 获取当前页的数据

			SearchSum = SearchList.Count;

			return Page();
		}

		/// <summary>
		/// 更新
		/// </summary>
		public async Task<IActionResult> OnPostUpdateVehicleAsync() {
			_logger.LogInformation("正在尝试使用ID更新车辆{VehicleId}", UpdatedVehicle.Id);

			//if (!ModelState.IsValid) {
			//	_logger.LogWarning("表单验证失败。VehicleId: {VehicleId}", UpdatedVehicle.Id);
			//	ErrorMessage = "表单验证失败，请检查输入内容";
			//	return Page();
			//}

			var result = await _vehicleController.UpdateVehicle(UpdatedVehicle);
			if (result is NotFoundResult) {
				_logger.LogWarning("找不到车辆。VehicleId: {VehicleId}", UpdatedVehicle.Id);
				ErrorMessage = "找不到车辆";
			} else if (result is StatusCodeResult status && status.StatusCode == 500) {
				_logger.LogError("更新车辆时出错。VehicleId: {VehicleId}", UpdatedVehicle.Id);
				ErrorMessage = "更新车辆时出错。";
			} else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
				_logger.LogWarning("使用ID更新车辆时发生并发冲突。VehicleId: {VehicleId}", UpdatedVehicle.Id);
				ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedVehicle.Id}";
			} else {
				_logger.LogInformation("ID为{VehicleId}的车辆已成功更新", UpdatedVehicle.Id);
				SuccessMessage = $"保存成功，已更新 ID：{UpdatedVehicle.Id}";
			}

			List = await _vehicleController.GetTablePage(Limit, PageIndex); // 刷新车辆列表

			Thread.Sleep(2500);

			if (!FileId.IsNullOrEmpty()) {
				_logger.LogInformation($"更新车辆图片信息FileId：{FileId}");
				// 更新车辆图片信息
				await _vehicleController.PutImagePath(UpdatedVehicle.Id, FileId);
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
                var sum = await _vehicleController.GetPageSum();

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
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostChangePageAsync([FromBody] Dictionary<string, int> requestData) {
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			PageIndex = requestData["PageIndex"];

			return new JsonResult(new { success = true, message = "成功", pageIndex = PageIndex });
		}

		/// <summary>
		/// 获取文件下载链接
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostImageDownload([FromBody] Dictionary<string, string> requestData) {
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
		/// 导出功能
		/// </summary>
		public async Task<IActionResult> OnGetExportToExcelAsync() {
			// 获取车辆表
			List<Vehicle> excelList = await _vehicleController.GetAllList();

			// 创建一个新的工作簿
			IWorkbook workbook = new XSSFWorkbook();
			ISheet sheet = workbook.CreateSheet("车辆数据");

			// 创建表头行
			IRow headerRow = sheet.CreateRow(0);
			headerRow.CreateCell(0).SetCellValue("序号");
			headerRow.CreateCell(1).SetCellValue("车辆 ID");
			headerRow.CreateCell(2).SetCellValue("车牌号");
			headerRow.CreateCell(3).SetCellValue("车主");
			headerRow.CreateCell(4).SetCellValue("车辆型号");
			headerRow.CreateCell(5).SetCellValue("创建时间");
			headerRow.CreateCell(6).SetCellValue("更新时间");

			// 填充数据
			for (int i = 0; i < excelList.Count; i++) {
				var row = sheet.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(i + 1);
				row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
				row.CreateCell(2).SetCellValue(excelList[i].PlateNumber ?? "");
				row.CreateCell(3).SetCellValue(excelList[i].Owner ?? "");
				row.CreateCell(4).SetCellValue(excelList[i].Model.ToString());
				row.CreateCell(5).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
				row.CreateCell(6).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
			}

			// 自动调整列宽
			for (int col = 0; col < 7; col++) {
				sheet.AutoSizeColumn(col);
			}

			// 将工作簿保存到内存流
			using (var memoryStream = new MemoryStream()) {
				workbook.Write(memoryStream);
				var fileName = "车辆数据.xlsx";
				var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

				// 返回文件流供下载
				return File(memoryStream.ToArray(), contentType, fileName);
			}
		}

		/// <summary>
		/// 获取查询数据
		/// </summary>
		public async Task<JsonResult> OnGetSearchDataAsync() {
			return new JsonResult(new { success = true, message = "成功", SearchPlateNumber, SearchOwner });
		}

		/// <summary>
		/// 更改查询数据
		/// </summary>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<JsonResult> OnPostChangeSearchDataAsync([FromBody] Dictionary<string, string> requestData) {
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

			SearchPlateNumber = requestData["SearchPlateNumber"];
			SearchOwner = requestData["SearchOwner"];

			return new JsonResult(new { success = true, message = "成功" });
		}
	}
}
