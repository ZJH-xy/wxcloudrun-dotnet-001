using aspnetapp.Models;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using Senparc.Weixin.MP.AdvancedAPIs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace aspnetapp.Controllers.API {

    [Route("user")]
    [ApiController]
    public class UserAPI : ControllerBase {
        private readonly UserController UserController = new(new MyDbContext());
        private readonly IOptionsSnapshot<JWTSettings> JWTSettingsOpt;

        public UserAPI(IOptionsSnapshot<JWTSettings> jWTSettingsOpt) {
            JWTSettingsOpt = jWTSettingsOpt;
        }

        // 获取用户基础信息
        [HttpGet("i/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            User? user;
            try {
                user = await UserController.GetUserById(id);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserById: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserBasic(user));
        }

        // 获取用户基础信息
        [HttpGet("p/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone) {
            User? user;
            try {
                user = await UserController.GetUserByPhone(phone);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserByPhone: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserBasic(user));
        }

        // 获取用户所有信息
        [HttpGet("a/i/{id}/{password}")]
        public async Task<IActionResult> GetUserPro(int id, string password) {
            if (password is null)
                return StatusCode(403, "密码为空");

            if (!(PasswordFormatDetermination(password)))
                return StatusCode(403, "密码格式错误");

            User? user;
            try {
                user = await UserController.GetUser(id);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUser: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            if (user.Password is null) 
                return StatusCode(403, "用户未设置密码");

            if (!(VerifyPassword(password, user.Password)))
                return StatusCode(403, "帐号或密码错误");

            // JWT
            List<Claim> claims = new() {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, "user")
            };

            return StatusCode(200, GetJwtToken(claims));
        }

        // 获取用户所有信息
        [HttpGet("a/p/{phone}/{password}")]
        public async Task<IActionResult> GetUserproByPhone(string phone, string password) {
            if (password is null)
                return StatusCode(403, "密码为空");

            if (!(PasswordFormatDetermination(password)))
                return StatusCode(403, "密码格式错误");

            User? user;
            try {
                user = await UserController.GetUserByPhone(phone);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]Login: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            if (user.Password is null)
                return StatusCode(403, "用户未设置密码");

            if (!(VerifyPassword(password, user.Password)))
                return StatusCode(403, "帐号或密码错误");

            // JWT
            List<Claim> claims = new() {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, "user")
            };

            return StatusCode(200, GetJwtToken(claims));
        }

        // 快速登录
        [HttpPost("ql/{code}")]
        public async Task<IActionResult> QuickLogin(string code) {
            var result = await BusinessApi.GetUserPhoneNumberAsync(BaseContainer<AccessTokenBag>.GetFirstOrDefaultAppId(PlatformType.WxOpen), code);

            switch (result.errcode) {
                case ReturnCode.请求成功:
                    break;

                case ReturnCode.系统繁忙此时请开发者稍候再试:
                    return StatusCode(403, "系统繁忙");

                case ReturnCode.不合法的oauth_code:
                    return StatusCode(403, "code 无效");

                case ReturnCode.不合法的APPID:
#if DEBUG
                    Console.WriteLine($"[错误]QuickLogin: 不合法的APPID，errcode: {result.errcode};");
#endif
                    return StatusCode(500);

                default:
#if DEBUG
                    Console.WriteLine($"[错误]QuickLogin: errcode: {result.errcode};");
#endif
                    return StatusCode(500);
            }

            User? user;
            try {
                user = await UserController.GetUserByPhone(result.phone_info.purePhoneNumber);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]QuickLogin 手机号获取用户: {e}");
#endif
                return StatusCode(500);
            }

            List<Claim> claims;
            // 未注册
            if (user is null) {
                int changeSum;
                user = new() {
                    Phone = result.phone_info.purePhoneNumber,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                try {
                    changeSum = await UserController.AddUser(user);

                } catch (Exception e) {
#if DEBUG
                    Console.WriteLine($"[错误]QuickLogin 新用户注册: {e}");
#endif
                    return StatusCode(500);
                }

                if (0 == changeSum) {
#if DEBUG
                    Console.WriteLine($"[异常]QuickLogin 用户注册失败，userJson: {user.ToJson()}");
#endif
                    return StatusCode(403, "注册失败，请联系管理员");
                }

                // JWT
                claims = new() {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "user")
                };

                return StatusCode(200, GetJwtToken(claims));
            }

            // 已注册
            claims = new() {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, "user")
            };

            return StatusCode(200, GetJwtToken(claims));
        }

        // 根据手机号获取收藏门店
        /*
        [HttpGet("f/s/{phone}")]
        public async Task<IActionResult> GetFavoriteStores(string phone) {            
            User? user = null;
            try {
                UserController.UserBasic? userBasic = await UserController.GetUserByPhone(phone);
                if (userBasic is null)
                    return StatusCode(404);

                user = await UserController.GetUser(userBasic.Value.UserId);
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]Login: {e}");
#endif
                return StatusCode(500);
            }
            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, new { favorite_stores = user.FavoriteStores.ToArray() });
        }*/

        /// <summary>
        /// JWT 令牌计算
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public string GetJwtToken(List<Claim> claims) {
            // 读取配置
            string key = JWTSettingsOpt.Value.SecKey;
            DateTime expires = DateTime.Now.AddDays(JWTSettingsOpt.Value.ExpireDays);
            // 计算
            byte[] secBytes = Encoding.UTF8.GetBytes(key);
            var secKey = new SymmetricSecurityKey(secBytes);
            var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(claims: claims,
                expires: expires, signingCredentials: credentials);
            string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return jwt;
        }

        /// <summary>
        /// 密码格式判断
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool PasswordFormatDetermination(string password) {
            return true;
            //if (password.Length < 8 ||  password.Length > 12)
            //    return false;

            //return true;
        }

        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string hashedPassword) {
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
        public static string HashPassword(string password) {
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

    public struct UserBasic {
        public UserBasic(User user) {
            UserId = user.UserId;
            Nickname = user.Nickname ?? string.Empty;
        }
        public int UserId { get; set; }
        public string Nickname { get; set; }
    }

    public struct UserPro {
        public UserPro(User user) {
            UserId = user.UserId;
            Phone = user.Phone;
            Name = user.Name;
            IdentityCard = user.IdentityCard;
            Nickname = user.Nickname;
        }
        public int UserId { get; set; }
        public string Phone { get; set; }
        public string? Name { get; set; }
        public string? IdentityCard { get; set; }
        public string? Nickname { get; set; }
    }
}
