using aspnetapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using Senparc.NeuChar.App.AppStore;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp.WxAppJson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Linq;

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
        [Authorize]
        [HttpGet("all/i")]
        public async Task<IActionResult> GetUserPro() {
            User? user;
            try {
                user = await UserController.GetUser(GetUserId());

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserPro(user));
        }

        // 实名认证
        [Authorize]
        [HttpPost("update/RealNameAuthentication")]
        public async Task<IActionResult> RealNameAuthentication(RealNameAuthentication real) {
            User? user;
            try {
                user = await UserController.GetUserById(GetUserId());

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro，获取用户: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if (!(user.Name is null || user.IdentityCard is null))// 已实名认证
                return StatusCode(403, "当前已实名认证");

            if (real.Name == string.Empty)
                return StatusCode(403, "请检查名字格式");

            if ((!Regex.IsMatch(real.IdentityCard, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase)))
                return StatusCode(403, "请检查身份证号格式");

            // 调用外部API 判断信息

            //

            int changSum = 0;
            user.Name = real.Name;
            user.IdentityCard = real.IdentityCard;
            try {
                changSum = await UserController.UpdateUser(user);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro，修改信息: {e}");
#endif
                return StatusCode(500);
            }
#if DEBUG
            Console.WriteLine($"[日志]GetUserPro，已修改行数：{changSum}");
#endif
            return StatusCode(200);
        }

        // 更新
        [Authorize]
        [HttpPost("update/i")]
        public async Task<IActionResult> UpdateUser(UpdateUser updateUser) {
            User? user;
            try {
                user = await UserController.GetUserById(updateUser.UserId);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if ((!Regex.IsMatch(updateUser.Phone, @"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$")))
                return StatusCode(403, "请检查手机号码格式");

            if (updateUser.Nickname.Length < 3 || updateUser.Nickname.Length > 6)
                return StatusCode(403, "请检查昵称格式");

            return StatusCode(200);
        }

        // 更改密码
        [Authorize]
        [HttpPost("update/password")]
        public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword) {
            User? user;
            string id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);// 获取用户id
            try {
                user = await UserController.GetUserById(int.Parse(id));

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro，获取用户: {e}");
#endif
                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if (user.Password is null)
                return StatusCode(403, "未实名认证");

            if (!(VerifyPassword(updatePassword.OldPassword, user.Password)))
                return StatusCode(403, "密码错误");

            int changSum = 0;
            try {
                user.Password = updatePassword.NewPassword;
                changSum = await UserController.UpdateUser(user);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserPro，密码修改: {e}");
#endif
                return StatusCode(500);
            }
#if DEBUG
            Console.WriteLine($"[日志]GetUserPro，已修改行数：{changSum}");
#endif

            return StatusCode(200);
        }

        // 登录
        [HttpGet("login/p/{phone}/{password}")]
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
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserId() {
            return int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

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
        /// 解码JWT
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string JwtDecode(string s) {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) {
                case 2:
                    s += "==";
                    break;
                case 3:
                    s += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
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
        /// <returns>等于true，否则false</returns>
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

    public class RealNameAuthentication {
        public string Name { get; set; }// 姓名
        public string IdentityCard { get; set; }// 身份证号
    }

    public class UpdateUser {
        public int UserId { get; init; }// 用户编号
        public string Phone { get; set; }// 手机号码
        public string Nickname { get; set; }// 昵称
    }

    public class UpdatePassword {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
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
