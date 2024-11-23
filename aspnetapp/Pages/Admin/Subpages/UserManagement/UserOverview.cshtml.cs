using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using Senparc.Weixin.WxOpen.Entities;
using System.IO;

namespace aspnetapp.Pages.Admin.Subpages.UserManagement {
    public class UserOverviewModel : PageModel {
        private readonly UserControllerWeb _userController;
        private readonly ILogger<UserOverviewModel> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public UserOverviewModel(UserControllerWeb userController, ILogger<UserOverviewModel> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _userController = userController;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public List<User> List { get; set; } = new List<User>();
        public class NewUser {
            public string Nickname { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public string IdentityCard { get; set; }
        }

        // 用于在页面显示错误信息
        public string ErrorMessage { get; set; }

        // 用于在页面显示成功信息
        public string SuccessMessage { get; set; }

        // 分页查询
        [BindProperty(SupportsGet = true)]
        public int Limit { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        // 更新
        [BindProperty]
        public User UpdatedUser { get; set; } = new User();

        // 查询
        [BindProperty(SupportsGet = true)]
        public string? SearchPhone { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchNickname { get; set; }

        // 排序
        [BindProperty(SupportsGet = true)]
        public string? SortField { get; set; } = "Id"; // 默认排序字段为 "Id"

        [BindProperty(SupportsGet = true)]
        public string? SortOrder { get; set; } = "asc"; // 默认排序顺序为升序

        // 图片上传
        public string ImageName { get; set; }// 图片文件名
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
                ImageName = "admin/user/identityCardPictures/" + Guid.NewGuid().ToString() + ".jpg";
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
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync() {
            _logger.LogInformation("正在获取限制为{Limit}的页面{PageIndex}的用户列表", PageIndex, Limit);
            try {
                List = await _userController.GetTablePage(Limit, PageIndex);

            } catch (Exception ex) {
                _logger.LogError(ex, "获取用户列表时出错");
                ModelState.AddModelError(string.Empty, "加载用户列表时发生错误。");
            }
            return Page();
        }

        /// <summary>
        /// 页码查询
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
		public async Task<IActionResult> OnGetPageAsync([FromBody] Dictionary<string, int> requestData) {
			// 确保接收到的数据被正确绑定
			if (requestData == null || !requestData.Any()) {
				return new JsonResult(new { success = false, message = "请求数据为空！" });
			}

            int limit = requestData["limit"];

            List = await _userController.SearchUsers(limit, SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            return new JsonResult(new { success = true, message = "查询成功" });
		}

		/// <summary>
		/// 更新
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostUpdateUserAsync() {
            _logger.LogInformation("正在尝试使用ID更新用户{UserId}", UpdatedUser.Id);

            if (!ModelState.IsValid) {
                _logger.LogWarning("表单验证失败。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "表单验证失败，请检查输入内容";
                return Page();
            }

            var result = await _userController.UpdateUser(UpdatedUser);
            if (result is NotFoundResult) {
                _logger.LogWarning("找不到用户。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "找不到用户";
            } else if (result is StatusCodeResult status && status.StatusCode == 500) {
                _logger.LogError("更新用户时出错。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = "更新用户时出错。";
            } else if (result is ObjectResult objResult && objResult.StatusCode == 409) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突。UserId: {UserId}", UpdatedUser.Id);
                ErrorMessage = $"您尝试编辑的记录已被其他用户修改。请重新加载数据后重试，ID：{UpdatedUser.Id}";
            } else {
                _logger.LogInformation("ID为{UserId}的用户已成功更新", UpdatedUser.Id);
                // 计算行号
                var rowIndex = List.FindIndex(user => user.Id == UpdatedUser.Id) + 1; // 行号从1开始
                SuccessMessage = $"保存成功，已更新 ID：{UpdatedUser.Id}";
            }

            List = await _userController.GetTablePage(Limit, PageIndex); // 刷新用户列表

            Thread.Sleep(2500);

            if (!FileId.IsNullOrEmpty()) {
#if DEBUG
                Console.WriteLine($"更新用户图片信息FileId：{FileId}");
#endif
                // 更新用户图片信息
                await _userController.PutImagePath(UpdatedUser.Id, FileId);
                FileId = "";
            }

            return Page();
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync() {
            _logger.LogDebug("Executing OnGetSearchAsync with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                           SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            // 调用 UserControllerWeb 中的 SearchUsers 方法，包含排序字段和顺序
            List = await _userController.SearchUsers(Limit, SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            return Page();
        }

		/// <summary>
        /// 返回总页数
        /// </summary>
        /// <returns></returns>
		public async Task<IActionResult> OnGetPageSumAsync() {
			try {
				// 获取所有用户数据的总数（可以通过服务方法获取）
				var sum = await _userController.GetPageSum();

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
		/// 导出功能
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnGetExportToExcelAsync() {
            // 获取用户表
			List<User> excelList = await _userController.GetAllList();

            // 创建一个新的工作簿
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("用户数据");

            // 创建表头行
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("序号");
            headerRow.CreateCell(1).SetCellValue("用户 ID");
            headerRow.CreateCell(2).SetCellValue("姓名");
            headerRow.CreateCell(3).SetCellValue("手机号");
            headerRow.CreateCell(4).SetCellValue("身份证号");
            headerRow.CreateCell(5).SetCellValue("昵称");
            headerRow.CreateCell(6).SetCellValue("创建时间");
            headerRow.CreateCell(7).SetCellValue("更新时间");

            // 填充数据
            for (int i = 0; i < excelList.Count; i++) {
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(i + 1);
                row.CreateCell(1).SetCellValue(excelList[i].Id.ToString());
                row.CreateCell(2).SetCellValue(excelList[i].Name ?? "");
                row.CreateCell(3).SetCellValue(excelList[i].Phone ?? "");
                row.CreateCell(4).SetCellValue(excelList[i].IdentityCard ?? "");
                row.CreateCell(5).SetCellValue(excelList[i].Nickname ?? "");
                row.CreateCell(6).SetCellValue(excelList[i].CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                row.CreateCell(7).SetCellValue(excelList[i].UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            // 自动调整列宽
            for (int col = 0; col < 8; col++) {
                sheet.AutoSizeColumn(col);
            }

            // 将工作簿保存到内存流
            using (var memoryStream = new MemoryStream()) {
                workbook.Write(memoryStream);
                var fileName = "用户数据.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                // 返回文件流供下载
                return File(memoryStream.ToArray(), contentType, fileName);
            }
        }
    }
}
