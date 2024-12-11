namespace aspnetapp.Controllers.API.Miniprogram {
    /// <summary>
    /// 主页相关
    /// </summary>
    [Route("home")]
    [ApiController]
    public class HomeAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly ILogger<HomeAPI> _logger;

        public HomeAPI(MyDbContext dbContext, ILogger<HomeAPI> logger) {
            _dbContext = dbContext;
            _logger = logger;
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
            var advertisementList = await _dbContext.Advertisement.Where(n => !n.IsDelete).ToListAsync();

            List<ReturnAdvertisement> returnNotice = new();

            foreach (var advertisement in advertisementList) {
                returnNotice.Add(new ReturnAdvertisement(advertisement));
            }

            return StatusCode(200, returnNotice);
        }
    }

    /// <summary>
    /// 返回公告格式
    /// </summary>
    public class ReturnNotice {
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
    public class ReturnAdvertisement {
        public ReturnAdvertisement(Advertisement advertisement) {
            Id = advertisement.Id;
            Title = advertisement.Title;
            Content = advertisement.Content;
            Src = advertisement.Src;
            CreatedAt = advertisement.CreatedAt;
            UpdatedAt = advertisement.UpdatedAt;
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Src { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
