namespace aspnetapp.Controllers.Web {
	public class LoginControllerWeb : Controller {
		private readonly MyDbContext _context;
		private readonly ILogger<LoginControllerWeb> _logger;

		public LoginControllerWeb(MyDbContext context, ILogger<LoginControllerWeb> logger) {
			_context = context;
			_logger = logger;
		}


		public bool Login(string account, string password) {
			var aa = _context.AdminAccount.SingleOrDefault(a => a.Account == account);
			if (aa is null) {
				return false;
			}

			if (aa.Password == password) {
				return true;
			}

			return false;
		}
	}
}
