using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Pages.Admin.Subpages.StoreManagement {
	[Authorize] // 确保需要认证才能访问
	[Authorize(Roles = "admin")]// 只有管理员能访问
	public class StoreAccountModel : PageModel {
        private readonly StoreAccountControllerWeb _storeAccountControllerWeb;
        private readonly ILogger<StoreAccountModel> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreAccountModel(StoreAccountControllerWeb _storeAccountControllerWeb, ILogger<StoreAccountModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            this._storeAccountControllerWeb = _storeAccountControllerWeb;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public List<StoreAccount> List { get; set; } = new();

        [BindProperty]
        public StoreAccount NewStore { get; set; } = new();

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
        public StoreAccount UpdatedStore { get; set; } = new();

        // 查询
        public static List<StoreAccount> SearchList { get; set; } = new();// 查询用List

        [BindProperty(SupportsGet = true)]
        public int SearchSum { get; set; } = 0;// 查询结果总数

        // 数据
        [BindProperty(SupportsGet = true)]
        public static int? SearchTheStore { get; set; }
        public string? SearchTheStoreHtml { get; set; }

        [BindProperty(SupportsGet = true)]
        public static string? SearchAccount { get; set; }
        public string? SearchAccountHtml { get; set; }

		// 排序
		[BindProperty(SupportsGet = true)]
		public static string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"
		public string? SortFieldHtml { get; set; }

		[BindProperty(SupportsGet = true)]
		public static string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序
		public string? SortOrderHtml { get; set; }

		// 图片上传
		public string ImageName { get; set; }// 图片文件名
        public static string FileId { get; set; } = "";


        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> OnPostAddStoreAsync() {
            await _storeAccountControllerWeb.AddStoreAccountAsync(NewStore);
            SuccessMessage = $"添加成功";
            List = await _storeAccountControllerWeb.GetTablePage(Limit, PageIndex); // 刷新用户列表

            return Page();
            //return await _storeControllerWeb.AddStoreAsync(NewStore);
        }

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


			return new JsonResult(new { success = true, message = $"已删除行数：{await _storeAccountControllerWeb.Delete(id)}" });
	    }

		/// <summary>
		/// 默认页码查询
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetAsync() {
            //_logger.LogInformation("[OnGetAsync]正在获取限制为{Limit}的页面{PageIndex}的用户列表", PageIndex, Limit);

            SearchTheStore = null;
            SearchAccount = null;
			SortField = "Id";
			SortOrder = "asc";

			try {
                List = await _storeAccountControllerWeb.GetTablePage(Limit, PageIndex);

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
			(List, SearchSum) = await _storeAccountControllerWeb.SearchStoreAccounts(Limit, PageIndex, SearchTheStore, SearchAccount, SortField, SortOrder);
			SuccessMessage = $"搜索成功";

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

            var result = await _storeAccountControllerWeb.UpdateStoreAccount(UpdatedStore);
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

            List = await _storeAccountControllerWeb.GetTablePage(Limit, PageIndex); // 刷新用户列表

            //Thread.Sleep(2500);

            return Page();
        }

        /// <summary>
        /// 返回总页数
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetPageSumAsync() {
            try {
                // 获取所有用户数据的总数（可以通过服务方法获取）
                var sum = await _storeAccountControllerWeb.GetPageSum();

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
        /// 导出商家帐号数据到 Excel
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetExportToExcelAsync() {
            // 获取商家帐号数据
            List<StoreAccount> excelList = await _storeAccountControllerWeb.GetAllList();

            // 创建一个新的工作簿
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("商家帐号数据");

            // 创建表头行
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序号");
            headerRow.CreateCell(1).SetCellValue("商家 ID");
            headerRow.CreateCell(2).SetCellValue("门店 ID");
            headerRow.CreateCell(3).SetCellValue("帐号");
            headerRow.CreateCell(4).SetCellValue("密码");
            headerRow.CreateCell(5).SetCellValue("并发版本");

            // 填充商家帐号数据
            for (int i = 0; i < excelList.Count; i++) {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1); // 序号
                row.CreateCell(1).SetCellValue(excelList[i].Id.ToString()); // 商家 ID
                row.CreateCell(2).SetCellValue(excelList[i].TheStore.ToString()); // 门店 ID
                row.CreateCell(3).SetCellValue(excelList[i].Account ?? ""); // 帐号
                row.CreateCell(4).SetCellValue(excelList[i].Password ?? ""); // 密码
                row.CreateCell(5).SetCellValue(Convert.ToBase64String(excelList[i].RowVersion)); // 并发版本
            }

			// 固定列宽
			for (int col = 0; col < 6; col++) {
				sheet.SetColumnWidth(col, 20 * 256); // 设置固定宽度，单位为 1/256 个字符宽度
			}

            // 将工作簿保存到内存流
            using (var memoryStream = new MemoryStream()) {
                workbook.Write(memoryStream);
                var fileName = "商家帐号数据.xlsx";
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

            SearchTheStore = int.Parse(requestData["SearchTheStore"]);
            SearchAccount = requestData["SearchAccount"];
			SortField = requestData["SortField"];
			SortOrder = requestData["SortOrder"];

			return new JsonResult(new { success = true, message = "成功" });
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> OnGetSearchDataAsync() {

            return new JsonResult(new { success = true, message = "成功", SearchTheStore, SearchAccount, SortField, SortOrder });
        }
    }
}
