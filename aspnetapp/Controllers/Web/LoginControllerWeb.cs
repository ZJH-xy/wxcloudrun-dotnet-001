using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace aspnetapp.Controllers.Web {
	public class LoginControllerWeb : Controller {
		private readonly MyDbContext _context;
		private readonly ILogger<LoginControllerWeb> _logger;
		private readonly IOptionsSnapshot<JWTSettings> _JWTSettingsOpt;

		public LoginControllerWeb(MyDbContext context, IOptionsSnapshot<JWTSettings> jWTSettingsOpt, ILogger<LoginControllerWeb> logger) {
			_context = context;
			_JWTSettingsOpt = jWTSettingsOpt;
			_logger = logger;
		}

		/// <summary>
		/// 登录判断
		/// </summary>
		/// <param name="account"></param>
		/// <param name="password"></param>
		/// <returns>真登录成功</returns>
		public async Task<(bool, string)> LoginAsync(string account, string password) {
			var adminAccount = await _context.AdminAccount.SingleOrDefaultAsync(a => a.Account == account);

			if (adminAccount is null) {
				return (false, "");
			}

			if (adminAccount.Password == password) {
				// 创建 Claim，添加自定义字段
				List<Claim> claims = new() {
					new Claim(ClaimTypes.NameIdentifier, adminAccount.Id.ToString()),
					new Claim(ClaimTypes.Name, adminAccount.Account),
					new Claim(ClaimTypes.Role, "admin")
				};

				return (true, GetJwtToken(claims, 12));
			}

			return (false, "");
		}

		/// <summary>
		/// 创建Claim
		/// </summary>
		/// <param name="id"></param>
		/// <param name="role">角色</param>
		/// <returns>List<Claim> 对象</returns>
		private List<Claim> CreateClaim(string id, string role) {
			List<Claim> claims = new() {
				new Claim(ClaimTypes.NameIdentifier, id),
				new Claim(ClaimTypes.Role, role)
			};
			return claims;
		}

		/// <summary>
		/// JWT 令牌计算
		/// </summary>
		/// <param name="claims"></param>
		/// <param name="hours">过期时间</param>
		/// <returns></returns>
		private string GetJwtToken(List<Claim> claims, int hours) {
			// 读取配置
			string key = _JWTSettingsOpt.Value.SecKey;
			//DateTime expires = DateTime.Now.AddDays(_JWTSettingsOpt.Value.ExpireDays);// 读取配置过期时间
			DateTime expires = DateTime.Now.AddHours(hours);// 过期时间

			// 计算
			byte[] secBytes = Encoding.UTF8.GetBytes(key);
			var secKey = new SymmetricSecurityKey(secBytes);
			var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);
			var tokenDescriptor = new JwtSecurityToken(claims: claims,
				expires: expires, signingCredentials: credentials);
			string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

			_logger.LogDebug("角色：{Role}，ID：{NameIdentifier}生成新JWTtoken：{claims}",
				claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
				claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
				claims.ToJToken());
			return jwt;
		}

		/// <summary>
		/// 验证密码
		/// </summary>
		/// <param name="password"></param>
		/// <param name="hashedPassword"></param>
		/// <returns>等于true，否则false</returns>
		private static bool VerifyPassword(string password, string hashedPassword) {
			// 对输入的密码进行SHA-256哈希
			//string hashedInputPassword = HashPassword(password);
			string hashedInputPassword = password;

			return hashedInputPassword == hashedPassword;
		}

		/// <summary>
		/// 使用SHA-256哈希密码
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		private static string HashPassword(string password) {
			using (SHA256 sha256 = SHA256.Create()) {
				// 将输入密码转换为字节数组
				byte[] bytes = Encoding.UTF8.GetBytes(password);

				// 计算哈希值
				byte[] hashBytes = sha256.ComputeHash(bytes);

				// 将哈希字节数组转换为16进制字符串
				StringBuilder builder = new StringBuilder();
				foreach (var b in hashBytes) {
					builder.Append(b.ToString("x2"));
				}

				// 返回哈希后的密码
				return builder.ToString();
			}
		}
	}
}
