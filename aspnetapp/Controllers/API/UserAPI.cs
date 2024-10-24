using NuGet.Protocol;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace aspnetapp.Controllers.API {

    [Route("user")]
    [ApiController]
    public class UserAPI : ControllerBase {
        UserController UserController = new(new MyDbContext());
        StoreController storeController = new(new MyDbContext());

        // 获取用户基础信息
        [HttpGet("i/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            User? user = null;
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
            User? user = null;
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
        [HttpGet("a/{id}")]
        public async Task<IActionResult> GetUserPro(int id, [FromBody] GetPassword data) {
            if (data.Password is null)
                return StatusCode(403, "密码为空");

            if (PasswordFormatDetermination(data.Password) == false)
                return StatusCode(403, "密码格式错误");

            User? user = null;
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

            if (VerifyPassword(data.Password, user.Password) == false)
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, new UserPro(user));
        }

        // 获取用户所有信息
        [HttpGet("l/{phone}")]
        public async Task<IActionResult> GetUserproByPhone(string phone, [FromBody] GetPassword data) {
            if (data.Password is null)
                return StatusCode(403, "密码为空");

            if (PasswordFormatDetermination(data.Password) == false)
                return StatusCode(403, "密码格式错误");

            User? user = null;
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

            if (VerifyPassword(data.Password, user.Password) == false)
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, new UserPro(user));
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

            User? user = null;
            try {
                user = await UserController.GetUserByPhone(result.phone_info.purePhoneNumber);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]QuickLogin 手机号获取用户: {e}");
#endif
                return StatusCode(500);
            }

            // 未注册
            if (user is null) {
                int changeSum = 0;
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

                return StatusCode(200, new UserBasic(user));
            }

            return StatusCode(200, new { user = new UserBasic(user), result.phone_info.purePhoneNumber });
        }

        // 根据手机号获取收藏门店
        [HttpGet("f/s/{phone}")]
        public async Task<IActionResult> GetFavoriteStores(string phone) {
            return StatusCode(404);
            /*
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
            */
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
            string hashedInputPassword = HashPassword(password);

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
    public struct GetPassword {
        public string Password { get; set; }
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
