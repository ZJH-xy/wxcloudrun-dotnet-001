namespace aspnetapp.Controllers.Web {
	public class StatisticsControllerWeb : Controller {

		private readonly MyDbContext _context;
		private readonly ILogger<StatisticsControllerWeb> _logger;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public StatisticsControllerWeb(MyDbContext context, ILogger<StatisticsControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
			_context = context;
			_logger = logger;
			_wxSetting = wxSetting;
		}

		public Dictionary<int, int> GetUser() {
			return new Dictionary<int, int> { };
		}
	}
}
