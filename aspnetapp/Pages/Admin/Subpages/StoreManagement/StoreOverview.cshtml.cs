using Microsoft.AspNetCore.Mvc.RazorPages;
using aspnetapp.Controllers.Web;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Pages.Admin.Subpages.StoreManagement {
    public class StoreOverviewModel : PageModel {
        private readonly StoreControllerWeb _storeControllerWeb;
        private readonly ILogger<StoreOverviewModel> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreOverviewModel(StoreControllerWeb storeController, ILogger<StoreOverviewModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _storeControllerWeb = storeController;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public List<Store> List { get; set; } = new();

        [BindProperty]
        public Store NewStore { get; set; } = new();

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
        public Store UpdatedStore { get; set; } = new Store();

        // 查询
        public static List<Store> SearchList { get; set; } = new();// 查询用List

        [BindProperty(SupportsGet = true)]
        public int SearchSum { get; set; } = 0;// 查询结果总数

        // 数据
        [BindProperty(SupportsGet = true)]
        public static string? SearchName { get; set; }
        public string? SearchNameHtml { get; set; }

        [BindProperty(SupportsGet = true)]
        public static string? SearchAddress { get; set; }
        public string? SearchAddressHtml { get; set; }

        // 排序
        [BindProperty(SupportsGet = true)]
        public string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序

        // 图片上传
        public string ImageName { get; set; }// 图片文件名
        public static string FileId { get; set; } = "";


		/// <summary>
		/// 添加
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> OnPostAddStoreAsync() {
			await _storeControllerWeb.AddStoreAsync(NewStore);
			SuccessMessage = $"添加成功";
			List = await _storeControllerWeb.GetTablePage(Limit, PageIndex); // 刷新用户列表

			return Page();
			//return await _storeControllerWeb.AddStoreAsync(NewStore);
		}

		/// <summary>
		/// 删除
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> OnDeletedeleteAsync([FromBody] Dictionary<string, string> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

            int id = int.Parse(requestData["id"]);
            if (id == 0)
                return new JsonResult(new { success = false, message = "ID不能为0"});

			
			return new JsonResult(new { success = true, message = $"已删除行数：{await _storeControllerWeb.DeleteStore(id)}"});
		}

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
                ImageName = "admin/store/pictures/" + Guid.NewGuid().ToString() + ".jpg";
                Console.WriteLine($"ImageName: {ImageName}");

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
        /// <param name="userId"></param>
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
        /// 默认页码查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync() {
            //_logger.LogInformation("[OnGetAsync]正在获取限制为{Limit}的页面{PageIndex}的用户列表", PageIndex, Limit);

            SearchName = "";
            SearchAddress = "";

            try {
                List = await _storeControllerWeb.GetTablePage(Limit, PageIndex);

            } catch (Exception ex) {
                _logger.LogError(ex, "获取用户列表时出错");
                ModelState.AddModelError(string.Empty, "加载用户列表时发生错误。");
            }
            return Page();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync() {

            // 调用 UserControllerWeb 中的 SearchUsers 方法，包含排序字段和顺序
            SearchList = await _storeControllerWeb.SearchStores(SearchName, SearchAddress);

            List = SearchList
                .Skip((PageIndex - 1) * Limit) // 跳过前面页的数据
                .Take(Limit).ToList(); // 获取当前页的数据

            SearchSum = SearchList.Count;

            return Page();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateUserAsync() {

            //if (!ModelState.IsValid) {
            //    _logger.LogWarning("表单验证失败。UserId: {UserId}", UpdatedStore.Id);
            //    ErrorMessage = "表单验证失败，请检查输入内容";
            //    return Page();
            //}

            var result = await _storeControllerWeb.UpdateStore(UpdatedStore);
            if (result is NotFoundResult) {
                _logger.LogWarning("找不到用户。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = "找不到用户";
            } else if (result is StatusCodeResult status && status.StatusCode == 500) {
                _logger.LogError("更新用户时出错。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = "更新用户时出错。";
            } else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedStore.Id}";
            } else {
                _logger.LogInformation("ID为{UserId}的用户已成功更新", UpdatedStore.Id);
                // 计算行号
                var rowIndex = List.FindIndex(user => user.Id == UpdatedStore.Id) + 1; // 行号从1开始
                SuccessMessage = $"保存成功，已更新 ID：{UpdatedStore.Id}";
            }

            List = await _storeControllerWeb.GetTablePage(Limit, PageIndex); // 刷新用户列表

            Thread.Sleep(2500);

            if (!FileId.IsNullOrEmpty()) {
#if DEBUG
                Console.WriteLine($"更新用户图片信息FileId：{FileId}");
#endif
                // 更新用户图片信息
                await _storeControllerWeb.PutStoreImagePath(UpdatedStore.Id, FileId);
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
                var sum = await _storeControllerWeb.GetPageSum();

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
        /// 导出门店数据到 Excel
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetExportToExcelAsync() {
            // 获取门店数据
            List<Store> excelList = await _storeControllerWeb.GetAllList();

            // 创建一个新的工作簿
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("门店数据");

            // 创建表头行
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序号");
            headerRow.CreateCell(1).SetCellValue("门店 ID");
            headerRow.CreateCell(2).SetCellValue("门店名称");
            headerRow.CreateCell(3).SetCellValue("联系电话");
            headerRow.CreateCell(4).SetCellValue("微信");
            headerRow.CreateCell(5).SetCellValue("地址");
            headerRow.CreateCell(6).SetCellValue("营业状态");
            headerRow.CreateCell(7).SetCellValue("营业开始时间");
            headerRow.CreateCell(8).SetCellValue("营业结束时间");
            headerRow.CreateCell(9).SetCellValue("创建时间");
            headerRow.CreateCell(10).SetCellValue("更新时间");

            // 填充门店数据
            for (int i = 0; i < excelList.Count; i++) {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1);
                row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
                row.CreateCell(2).SetCellValue(excelList[i].Name ?? "");
                row.CreateCell(3).SetCellValue(excelList[i].Telephone ?? "");
                row.CreateCell(4).SetCellValue(excelList[i].WeChat ?? "");
                row.CreateCell(5).SetCellValue(excelList[i].Address ?? "");
                row.CreateCell(6).SetCellValue(excelList[i].BusinessStatus ? "营业中" : "关闭");
                row.CreateCell(7).SetCellValue(excelList[i].BusinessHoursStart.ToString(@"hh\:mm"));
                row.CreateCell(8).SetCellValue(excelList[i].BusinessHoursEnd.ToString(@"hh\:mm"));
                row.CreateCell(9).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                row.CreateCell(10).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            // 自动调整列宽
            for (int col = 0; col < 11; col++) {
                sheet.AutoSizeColumn(col);
            }

            // 将工作簿保存到内存流
            using (var memoryStream = new MemoryStream()) {
                workbook.Write(memoryStream);
                var fileName = "门店数据.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // 返回文件流供下载
                return File(memoryStream.ToArray(), contentType, fileName);
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

            SearchName = requestData["SearchName"];
            SearchAddress = requestData["SearchAddress"];

            return new JsonResult(new { success = true, message = "成功" });
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> OnGetSearchDataAsync() {

            return new JsonResult(new { success = true, message = "成功", SearchName, SearchAddress });
        }
    }
}
