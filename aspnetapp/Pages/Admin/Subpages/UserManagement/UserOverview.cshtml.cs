using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.Web;
using aspnetapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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
        public string FilePath2 { get; set; }// 图片文件名

        // 所需信息
        public string ImageUrl { get; set; }
        public string FilePath { get; set; }
        public string Authorization { get; set; }
        public string Token { get; set; }
        public string Cos_file_id { get; set; }
        // 用于提供给前端的 API 方法
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
                ImageName = Guid.NewGuid().ToString() + "_test.jpg";

                WxUploadFileJsonResult result = await TcbApi.UploadFileAsync(appId, envId, ImageName);

                if (result.ErrorCodeValue != 0) {
                    message = "上传失败，错误码：" + result.ErrorCodeValue;
                    Console.WriteLine(message);
                    return new JsonResult(new { success = false, message });
                }

                string filePath = @$"admin/user/identityCardPictures/{ImageName}";
                Console.WriteLine("文件上传成功！");

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

            //if (string.IsNullOrEmpty(ImageName))
            //	return StatusCode(403, "文件名错误");

            // 设置微信小程序的AppId和AppSecret
            var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
            var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
            var envId = _wxSetting.Value.Env;

            ImageName = Guid.NewGuid().ToString() + "_" + UpdatedUser.Id + ".png";

            WxUploadFileJsonResult resultImage;
            try {
                // 获取上传文件路径
                resultImage = await TcbApi.UploadFileAsync(appId, envId, ImageName);

                if (resultImage.ErrorCodeValue != 0) {
                    Console.WriteLine($"错误{resultImage.ToJson()}");
                    return StatusCode(500);
                }
                // 输出上传结果
                Console.WriteLine("File uploaded successfully!");
                Console.WriteLine("File ID: " + resultImage.file_id);

            } catch (Exception ex) {
                // 输出错误信息
                Console.WriteLine("获取上传文件路径：" + ex.Message);
                return StatusCode(500);
            }

            string filePath = @"7072-prod-3g2khb36f729f21f-1331625129/admin/user/identityCardPictures/" + ImageName;// 用户身份证文件路径

            ImageUrl = resultImage.url;
            FilePath = filePath;
            Authorization = resultImage.authorization;// 签名
            Token = resultImage.token;
            Cos_file_id = resultImage.cos_file_id;

            //return StatusCode(200);

            List = await _userController.GetTablePage(Limit, PageIndex); // 刷新用户列表

            return Page();
        }

        /// <summary>
        /// 获取文件上传所需信息
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostUpdateUserImageAsync() {
            //设置微信小程序的AppId和AppSecret
            //var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
            //var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
            //var envId = "prod-3g2khb36f729f21f";

            //WxUploadFileJsonResult result = await TcbApi.UploadFileAsync(appId, envId, "1.txt");

            //return StatusCode(200);

            //if (string.IsNullOrEmpty(ImageName))
            //    return StatusCode(403, "文件名错误");

            // 设置微信小程序的AppId和AppSecret
            var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
            var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
            var envId = _wxSetting.Value.Env;

            ImageName = Guid.NewGuid().ToString() + "_" + UpdatedUser.Id + ".png";

            WxUploadFileJsonResult result;
            try {
                // 获取上传文件路径
                result = await TcbApi.UploadFileAsync(appId, envId, ImageName);

                if (result.ErrorCodeValue != 0) {
                    Console.WriteLine($"错误{result.ToJson()}");
                    return StatusCode(500);
                }
                // 输出上传结果
                Console.WriteLine("File uploaded successfully!");
                Console.WriteLine("File ID: " + result.file_id);

            } catch (Exception ex) {
                // 输出错误信息
                Console.WriteLine("获取上传文件路径：" + ex.Message);
                return StatusCode(500);
            }

            string filePath = @"7072-prod-3g2khb36f729f21f-1331625129/admin/user/identityCardPictures/" + ImageName;// 用户身份证文件路径

            ImageUrl = result.url;
            FilePath = filePath;
            Authorization = result.authorization;// 签名
            Token = result.token;
            Cos_file_id = result.cos_file_id;

            return Page();

            //return await _userController.GetUploadPath(ImageName);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostSearchAsync() {
            _logger.LogDebug("Executing OnGetSearchAsync with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                           SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            // 调用 UserControllerWeb 中的 SearchUsers 方法，包含排序字段和顺序
            List = await _userController.SearchUsers(SearchPhone, SearchName, SearchNickname, SortField, SortOrder);
            return Page();
        }

    }
}
