using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Models;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.API.Miniprogram {
    /// <summary>
    /// 主页相关
    /// </summary>
    [Route("home")]
    [ApiController]
    public class HomeAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly ILogger<HomeAPI> _logger;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public HomeAPI(MyDbContext dbContext, ILogger<HomeAPI> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _dbContext = dbContext;
            _logger = logger;
			_wxSetting = wxSetting;
		}

        /// <summary>
        /// 获取公告
        /// </summary>
        [HttpGet("notice")]
        public async Task<IActionResult> GetNotice() {
            var noticeList = await _dbContext.Notice.Where(n => !n.IsDelete).ToListAsync();

            List<ReturnNotice> returnNotice = new();

            foreach (var notice in noticeList) {
                returnNotice.Add(new ReturnNotice(notice));
            }

            return StatusCode(200, returnNotice);
        }

        /// <summary>
        /// 获取广告
        /// </summary>
        [HttpGet("advertisement")]
        public async Task<IActionResult> GetAdvertisement() {
			List<Advertisement> adList;
			try {
                adList = await _dbContext.Advertisement.Where(n => !n.IsDelete).ToListAsync();

			} catch (Exception e) {
				_logger.LogError(e, "获取所有广告失败");

				return StatusCode(500);
			}


            List<ReturnAdvertisement> returnNotice = new();

            foreach (Advertisement ad in adList) {

				ReturnAdvertisement returnAdvertisement = new(ad);

				// 获取图片下载
				returnAdvertisement.Pictures = returnAdvertisement.Pictures is not null ? await GetImageDownload(returnAdvertisement.Pictures) : null;

				returnNotice.Add(returnAdvertisement);
			}

			return StatusCode(200, returnNotice);
        }

		/// <summary>
		/// 获取图片文件下载链接
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task<string> GetImageDownload(string fileid) {
			var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			//var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
			var envId = _wxSetting.Value.Env;

			List<FileItem> fileid_list = new() {
				new FileItem {
					fileid = fileid,
					max_age = 7200
				}
				};

			var re = await TcbApi.BatchDownloadFileAsync(appId, envId, fileid_list);

			if (re.errcode != ReturnCode.请求成功) {
				_logger.LogError("{errmsg},获取下载链接{fileid_list}", re.errmsg, fileid_list.ToJson());
			}

			return re.file_list.First().download_url;
		}
	}

    /// <summary>
    /// 返回公告格式
    /// </summary>
    public struct ReturnNotice {
        public ReturnNotice(Notice notice) {
            Id = notice.Id;
            Title = notice.Title;
            Content = notice.Content;
            CreatedAt = notice.CreatedAt;
            UpdatedAt = notice.UpdatedAt;
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    /// <summary>
    /// 返回广告格式
    /// </summary>
    public struct ReturnAdvertisement {
        public ReturnAdvertisement(Advertisement advertisement) {
            Id = advertisement.Id;
            Title = advertisement.Title;
            Content = advertisement.Content;
            Pictures = advertisement.Pictures;
			Src = advertisement.Src;
            CreatedAt = advertisement.CreatedAt;
            UpdatedAt = advertisement.UpdatedAt;
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
		public string? Pictures { get; set; }
		public string Src { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
