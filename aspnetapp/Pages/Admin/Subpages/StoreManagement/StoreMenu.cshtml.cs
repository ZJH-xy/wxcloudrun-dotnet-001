using aspnetapp.Controllers.Web;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using aspnetapp.Controllers.Miniprogram;

namespace aspnetapp.Pages.Admin.Subpages.StoreManagement {
    public class StoreMenuModel : PageModel {
        private readonly StoreMenuControllerWeb _storeMenuControllerWeb;
        private readonly ILogger<StoreMenuModel> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreMenuModel(StoreMenuControllerWeb storeController, ILogger<StoreMenuModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _storeMenuControllerWeb = storeController;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public List<StoreMenu> List { get; set; } = new();

        [BindProperty]
        public StoreMenu NewStoreMenu { get; set; } = new();

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
        public StoreMenu UpdatedStore { get; set; } = new StoreMenu();

        // 查询
        public static List<StoreMenu> SearchList { get; set; } = new();// 查询用List

        [BindProperty(SupportsGet = true)]
        public int SearchSum { get; set; } = 0;// 查询结果总数

        // 数据
        [BindProperty(SupportsGet = true)]
        public static int? SearchTheStore { get; set; }
        public int? SearchTheStoreHtml { get; set; }

        // 排序
        [BindProperty(SupportsGet = true)]
        public string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序

        /// <summary>
        /// 添加
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> OnPostAddStoreAsync() {
            await _storeMenuControllerWeb.AddStoreAsync(NewStoreMenu);
            SuccessMessage = $"添加成功";
            List = await _storeMenuControllerWeb.GetTablePage(Limit, PageIndex); // 刷新门店列表

            return Page();
            //return await _storeMenuControllerWeb.AddStoreAsync(NewStore);
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


            return new JsonResult(new { success = true, message = $"已删除行数：{await _storeMenuControllerWeb.DeleteStore(id)}" });
        }

        /// <summary>
        /// 默认页码查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync() {
            //_logger.LogInformation("[OnGetAsync]正在获取限制为{Limit}的页面{PageIndex}的门店列表", PageIndex, Limit);

            SearchTheStore = null;

            try {
                List = await _storeMenuControllerWeb.GetTablePage(Limit, PageIndex);

            } catch (Exception ex) {
                _logger.LogError(ex, "获取门店列表时出错");
                ModelState.AddModelError(string.Empty, "加载门店列表时发生错误。");
            }
            return Page();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync() {

            // 调用 UserControllerWeb 中的 SearchUsers 方法，包含排序字段和顺序
			(List, SearchSum) = await _storeMenuControllerWeb.SearchStoreMenus(Limit, PageIndex, SearchTheStore, SortField, SortOrder);

            return Page();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateAsync() {

            //if (!ModelState.IsValid) {
            //    _logger.LogWarning("表单验证失败。UserId: {UserId}", UpdatedStore.Id);
            //    ErrorMessage = "表单验证失败，请检查输入内容";
            //    return Page();
            //}

            var result = await _storeMenuControllerWeb.UpdateStoreMenu(UpdatedStore);
            if (result is NotFoundResult) {
                _logger.LogWarning("找不到门店。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = "找不到门店";
            } else if (result is StatusCodeResult status && status.StatusCode == 500) {
                _logger.LogError("更新门店时出错。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = "更新门店时出错。";
            } else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
                _logger.LogWarning("使用ID更新门店时发生并发冲突。UserId: {UserId}", UpdatedStore.Id);
                ErrorMessage = $"您尝试编辑的记录已被其他门店修改。请重新加载数据后重试，ID：{UpdatedStore.Id}";
            } else {
                _logger.LogInformation("ID为{UserId}的门店已成功更新", UpdatedStore.Id);
                // 计算行号
                var rowIndex = List.FindIndex(user => user.Id == UpdatedStore.Id) + 1; // 行号从1开始
                SuccessMessage = $"保存成功，已更新 ID：{UpdatedStore.Id}";
            }

            List = await _storeMenuControllerWeb.GetTablePage(Limit, PageIndex); // 刷新门店列表

            return Page();
        }

        /// <summary>
        /// 返回总页数
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetPageSumAsync() {
            try {
                // 获取所有门店数据的总数（可以通过服务方法获取）
                var sum = await _storeMenuControllerWeb.GetPageSum();

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
        /// 导出门店套餐数据到 Excel
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetExportToExcelAsync() {
            // 获取门店套餐数据
            List<StoreMenu> excelList = await _storeMenuControllerWeb.GetAllList();

            // 创建一个新的工作簿
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("门店套餐数据");

            // 创建表头行
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序号");
            headerRow.CreateCell(1).SetCellValue("门店套餐 ID");
            headerRow.CreateCell(2).SetCellValue("门店 ID");
            headerRow.CreateCell(3).SetCellValue("时长（小时）");
            headerRow.CreateCell(4).SetCellValue("租金（元）");
            headerRow.CreateCell(5).SetCellValue("押金（元）");
            headerRow.CreateCell(6).SetCellValue("并发版本");

            // 填充门店套餐数据
            for (int i = 0; i < excelList.Count; i++) {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1); // 序号
                row.CreateCell(1).SetCellValue(excelList[i].Id.ToString()); // 门店套餐 ID
                row.CreateCell(2).SetCellValue(excelList[i].TheStore.ToString()); // 门店 ID
                row.CreateCell(3).SetCellValue(excelList[i].Duration.ToString()); // 时长
                row.CreateCell(4).SetCellValue((double)excelList[i].Rent); // 租金
                row.CreateCell(5).SetCellValue((double)excelList[i].Deposit); // 押金
                row.CreateCell(6).SetCellValue(Convert.ToBase64String(excelList[i].RowVersion)); // 并发版本
            }

			// 固定列宽
			for (int col = 0; col < 6; col++) {
				sheet.SetColumnWidth(col, 20 * 256); // 设置固定宽度，单位为 1/256 个字符宽度
			}

            // 将工作簿保存到内存流
            using (var memoryStream = new MemoryStream()) {
                workbook.Write(memoryStream);
                var fileName = "门店套餐数据.xlsx";
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
        public async Task<JsonResult> OnPostChangeSearchDataAsync([FromBody] Dictionary<string, int> requestData) {
            // 确保接收到的数据被正确绑定
            if (requestData == null || !requestData.Any()) {
                return new JsonResult(new { success = false, message = "请求数据为空！" });
            }

            SearchTheStore = requestData["SearchTheStore"];

            return new JsonResult(new { success = true, message = "成功" });
        }

        /// <summary>
        /// 获取查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> OnGetSearchDataAsync() {

            return new JsonResult(new { success = true, message = "成功", SearchTheStore });
        }
    }
}
