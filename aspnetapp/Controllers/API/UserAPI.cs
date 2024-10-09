using System.Security.Cryptography;
using System.Text;

namespace aspnetapp.Controllers.API {
    [Route("user")]
    [ApiController]
    public class UserAPI : ControllerBase {
        UserController UserController = new UserController(new MyDbContext());

        [HttpGet("i/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            try {
                UserController.UserBasic? user = await UserController.GetUserById(id);
                if (user is null) {
                    return StatusCode(404);
                } else {
                    return StatusCode(200, new { user_basic = user.ToJson() });
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserById: {e}");
#endif
                return StatusCode(500, new { message = "服务器错误", code = 500 });
            }
        }

        [HttpGet("p/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone) {
            try {
                UserController.UserBasic? user = await UserController.GetUserByPhone(phone);
                if (user is null) {
                    return StatusCode(404);
                } else {
                    return StatusCode(200, new { user_basic = user.ToJson() });
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserByPhone: {e}");
#endif
                return StatusCode(500, new { message = "服务器错误", code = 500 });
            }
        }

        [HttpGet("a/{id}/{password}")]
        public async Task<IActionResult> GetUser(int id, string password) {
            try {
                User? user = await UserController.GetUser(id);

                if (user is null)
                    return StatusCode(404);

                if (user.Password is null)
                    return StatusCode(403, "非法请求");

                if (VerifyPassword(password, user.Password)) {
                    return StatusCode(200, new {user_pro = new UserController.UserPro(user)});
                } else {
                    return StatusCode(403, "帐号或密码错误");
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUser: {e}");
#endif
                return StatusCode(500, new { message = "服务器错误", code = 500 });
            }
        }

        [HttpPut("add")]
        public async Task<IActionResult> AddUser() {
            try {
                throw new NotImplementedException();
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddUser: {e}");
#endif
                return StatusCode(500, new { message = "服务器错误", code = 500 });
            }
        }

        // 根据手机号获取收藏门店

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

                return builder.ToString(); // 返回哈希后的密码
            }
        }
    }
}
