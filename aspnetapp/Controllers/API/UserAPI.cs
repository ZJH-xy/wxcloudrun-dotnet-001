using Senparc.CO2NET;
using Senparc.Weixin;
using Senparc.Weixin.Containers;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using System.Security.Cryptography;
using System.Text;

namespace aspnetapp.Controllers.API {
    [Route("user")]
    [ApiController]
    [ApiBind]
    public class UserAPI : ControllerBase {
        UserController UserController = new UserController(new MyDbContext());
        StoreController storeController = new StoreController(new MyDbContext());

        // 添加用户（测试）
        [HttpPost("add")]
        public async Task<IActionResult> PostUser(GetUser getUser) {// [FromBody]
            try {
                User user = new User() {
                    Phone = getUser.Phone,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                int changesSum = await UserController.AddUser(user);
                return StatusCode(201, new { change_sum = changesSum });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]PostUserAsync: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }


        [HttpGet("i/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            try {
                UserController.UserBasic? user = await UserController.GetUserById(id);
                if (user is null)
                    return StatusCode(404);

                return StatusCode(200, new { user_basic = user.ToJson() });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserById: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }

        [HttpGet("p/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone) {
            try {
                UserController.UserBasic? user = await UserController.GetUserByPhone(phone);
                if (user is null)
                    return StatusCode(404);

                return StatusCode(200, new { user_basic = user.ToJson() });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserByPhone: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }

        [HttpGet("a/{id}/{password}")]
        public async Task<IActionResult> GetUser(int id, string password) {
            try {
                User? user = await UserController.GetUser(id);

                if (user is null)
                    return StatusCode(403, "帐号或密码错误");

                if (user.Password is null)
                    return StatusCode(403, "非法请求");

                if (!VerifyPassword(password, user.Password))
                    StatusCode(403, "帐号或密码错误");

                return StatusCode(200, new { user_pro = new UserController.UserPro(user).ToJson() });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUser: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }

        [HttpPost("l/{phone}/{password}")]
        public async Task<IActionResult> Login(string phone, string password) {
            if (password is null)
                return StatusCode(400, "密码为空");

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
                return StatusCode(500, "服务器错误");
            }
            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            if (user.Password is null)
                return StatusCode(403, "未设置密码");

            if (!VerifyPassword(password, user.Password))
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, new { user_pro = new UserController.UserPro(user).ToJson() });
        }

        // 快速登录
        [HttpPost("ql/{code}")]
        public async Task<IActionResult> QuickLogin(string code) {
            var result = await BusinessApi.GetUserPhoneNumberAsync(BaseContainer<AccessTokenBag>.GetFirstOrDefaultAppId(PlatformType.WxOpen), code);
            switch (result.errcode) {
                case ReturnCode.请求成功:
                    break;

                case ReturnCode.系统繁忙此时请开发者稍候再试:
                    return StatusCode(408, "系统繁忙");

                case ReturnCode.不合法的oauth_code:
                    return StatusCode(403, "code 无效");

                case ReturnCode.不合法的APPID:
#if DEBUG
                    Console.WriteLine($"[错误]in QuickLogin errcode is {result.errcode}");
#endif
                    return StatusCode(500, "服务器错误");

                default:
#if DEBUG
                    Console.WriteLine($"[错误]in QuickLogin errcode is {result.errcode}");
#endif
                    return StatusCode(500, "服务器错误");
            }

            UserController.UserBasic? userBasic = await UserController.GetUserByPhone(result.phone_info.purePhoneNumber);

            // 未注册
            if (userBasic is null) {
                User user = new();
                user.Phone = result.phone_info.purePhoneNumber;
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;

                try {
                    int ChangeSum = await UserController.AddUser(user);

                    if (ChangeSum > 0) {
                        return StatusCode(200, new { use_basic = new UserController.UserBasic(user).ToJson() });
                    }
#if DEBUG
                    Console.WriteLine($"[错误]注册用户未成功插入数据库");
#endif
                    return StatusCode(406, "注册失败，请联系管理员");
                } catch (Exception e) {
#if DEBUG
                    Console.WriteLine($"[错误]注册新用户QuickLogin: {e}");
#endif
                    return StatusCode(500, "服务器错误");
                }
            }

            return StatusCode(200, new { use_basic = userBasic.ToJson(), purePhoneNumber = result.phone_info.purePhoneNumber });
        }

        // 根据手机号获取收藏门店
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
                return StatusCode(500, "服务器错误");
            }
            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, new { favorite_stores = user.FavoriteStores.ToArray() });
        }

        // 验证密码
        public static bool VerifyPassword(string password, string hashedPassword) {
            // 对输入的密码进行SHA-256哈希
            string hashedInputPassword = HashPassword(password);

            return hashedInputPassword == hashedPassword;
        }

        // 使用SHA-256哈希密码
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
    public class GetUser {
        public string Phone { get; set; }
    }
}
