using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using aspnetapp.Controllers.Miniprogram;

namespace aspnetapp.Controllers.API.Miniprogram {

    [Route("user")]
    [ApiController]
    public class UserAPI : ControllerBase {
        private readonly UserController UserController = new(new MyDbContext());
        private readonly FavoritesStoreController favoritesStoreController = new(new MyDbContext());
        private readonly IOptionsSnapshot<JWTSettings> _JWTSettingsOpt;
        private readonly ILogger<OrderAPI> _logger;

        public UserAPI(IOptionsSnapshot<JWTSettings> jWTSettingsOpt, ILogger<OrderAPI> logger) {
            _JWTSettingsOpt = jWTSettingsOpt;
            _logger = logger;
        }

        /// <summary>
        /// Id获取用户基础信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetUserById(int id) {
            User? user;
            try {
                user = await UserController.GetUserById(id);

            } catch (Exception e) {
                _logger.LogError(e, "Id获取用户{UserId}基础信息", id);

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserBasic(user));
        }

        /// <summary>
        /// 手机号获取用户基础信息
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetUserByPhone(string phone) {
            User? user;
            try {
                user = await UserController.GetUserByPhone(phone);

            } catch (Exception e) {
                _logger.LogError(e, "手机号获取用户{Phone}基础信息", phone);

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserBasic(user));
        }

        /// <summary>
        /// 获取用户所有信息
        /// </summary>
        /// <returns></returns>
        [Authorize]// 方法受到限制
        [HttpGet("all/i")]
        public async Task<IActionResult> GetUserPro() {
            User? user;
            try {
                user = await UserController.GetUser(GetUserIdInt());

            } catch (Exception e) {
                _logger.LogError(e, "获取用户{UserId}所有信息", GetUserIdInt());

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            return StatusCode(200, new UserPro(user));
        }

        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="real"></param>
        /// <returns></returns>
        [Authorize]// 方法受到限制
        [HttpPost("update/RealNameAuthentication")]
        public async Task<IActionResult> RealNameAuthentication(RealNameAuthentication real) {
            User? user;
            try {
                user = await UserController.GetUserById(GetUserIdInt());

            } catch (Exception e) {
                _logger.LogError(e, "获取用户{UserId}", GetUserIdInt());

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if (user.Name is null || user.IdentityCard is null)// 已实名认证
                return StatusCode(403, "当前已实名认证");

            if (!Judge.NameFormatDetermination(real.Name))
                return StatusCode(403, "请检查名字格式");

            if (!Regex.IsMatch(real.IdentityCard, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase))
                return StatusCode(403, "请检查身份证号格式");

            // 调用外部API 判断信息正确性

            //

            int changSum;
            user.Name = real.Name;
            user.IdentityCard = real.IdentityCard;
            try {
                changSum = await UserController.UpdateUser(user);

            } catch (Exception e) {
                _logger.LogError(e, "用户{UserId}实名认证，更新数据", GetUserIdInt());

                return StatusCode(500);
            }
            _logger.LogInformation("用户{UserId}实名认证成功，已修改行数{ChangSum}", GetUserIdInt(), changSum);

            return StatusCode(200);
        }

        /// <summary>
        /// 更新（昵称、手机号）
        /// </summary>
        /// <param name="updateUser"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("update/i")]
        public async Task<IActionResult> UpdateUser(UpdateUser updateUser) {
            User? user;
            try {
                user = await UserController.GetUserById(GetUserIdInt());

            } catch (Exception e) {
                _logger.LogError(e, "获取用户{UserId}", GetUserIdInt());

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if (!Judge.PhoneFormatDetermination(updateUser.Phone))
                return StatusCode(403, "请检查手机号码格式");

            if (updateUser.Nickname.Length < 2 || updateUser.Nickname.Length > 8)
                return StatusCode(403, "请检查昵称格式");

            return StatusCode(200);
        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="updatePassword"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("update/password")]
        public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword) {
            User? user;
            try {
                user = await UserController.GetUserById(GetUserIdInt());

            } catch (Exception e) {
                _logger.LogError(e, "获取用户{UserId}", GetUserIdInt());

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(404);

            if (user.Password is null)
                return StatusCode(403, "未实名认证");

            if (!(updatePassword.OldPassword == user.Password))
                return StatusCode(403, "密码错误");

            int changSum = 0;
            try {
                user.Password = updatePassword.NewPassword;
                changSum = await UserController.UpdateUser(user);

            } catch (Exception e) {
                _logger.LogError(e, "用户{UserId}修改密码", GetUserIdInt());

                return StatusCode(500);
            }
            _logger.LogInformation("用户{UserId}修改密码成功，已修改行数{ChangSum}", GetUserIdInt(), changSum);

            return StatusCode(200);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpGet("login/phone/{phone}/{password}")]
        public async Task<IActionResult> GetUserproByPhone(string phone, string password) {
            if (password is null)
                return StatusCode(403, "密码为空");

            if (!Judge.PasswordFormatDetermination(password))
                return StatusCode(403, "密码格式错误");

            User? user;
            try {
                user = await UserController.GetUserByPhone(phone);

            } catch (Exception e) {
                _logger.LogError(e, "用户{UserId}登录", GetUserIdInt());

                return StatusCode(500);
            }

            if (user is null)
                return StatusCode(403, "帐号或密码错误");

            if (user.Password is null)
                return StatusCode(403, "用户未设置密码");

            if (!(password == user.Password))
                return StatusCode(403, "帐号或密码错误");

            return StatusCode(200, GetJwtToken(CreateClaim(user.Id.ToString(), "user")));
        }

        /// <summary>
        /// 快速登录
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
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
                    _logger.LogCritical("用户快速登录，errcode：{Errcode}", result.errcode);

                    return StatusCode(500);

                default:
                    _logger.LogError("用户快速登录，errcode：{Errcode}", result.errcode);

                    return StatusCode(500);
            }

            User? user;
            try {
                user = await UserController.GetUserByPhone(result.phone_info.purePhoneNumber);

            } catch (Exception e) {
                _logger.LogError(e, "手机号获取用户{Phone}", result.phone_info.purePhoneNumber);

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
                    if (0 == changeSum)
                        throw new Exception("新增行数为0");

                } catch (Exception e) {
                    _logger.LogError(e, "新用户注册");

                    return StatusCode(403, "注册失败，请联系管理员");
                }

                return StatusCode(200, GetJwtToken(CreateClaim(user.Id.ToString(), "user")));
            }

            // 已注册
            return StatusCode(200, GetJwtToken(CreateClaim(user.Id.ToString(), "user")));
        }

        /// <summary>
        /// 获取收藏门店
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("favorites/store")]
        public async Task<IActionResult> GetFavoriteStores() {
            return StatusCode(200, await favoritesStoreController.GetByUserId(GetUserIdInt()));
        }

        /// <summary>
        /// 删除收藏门店DelFavoritesStore
        /// </summary>
        /// <param name="favoritesId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete("favorites/store/{favoritesId}")]
        public async Task<IActionResult> DelFavoritesStore(int favoritesId) {
            var uf = await favoritesStoreController.GetById(favoritesId);

            if (uf is null || uf.TheUser != GetUserIdInt())
                return StatusCode(401);

            await favoritesStoreController.DelFavoritesStore(favoritesId);
            return StatusCode(200);
        }

        /// <summary>
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserIdInt() {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // JWT 获取用户id
        public string GetUserIdString() {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// 创建Claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role">角色</param>
        /// <returns>List<Claim> 对象</returns>
        public List<Claim> CreateClaim(string id, string role) {
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
        /// <returns></returns>
        public string GetJwtToken(List<Claim> claims) {
            // 读取配置
            string key = _JWTSettingsOpt.Value.SecKey;
            DateTime expires = DateTime.Now.AddDays(_JWTSettingsOpt.Value.ExpireDays);// 读取配置过期时间

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
        public string Phone { get; set; }// 手机号码
        public string Nickname { get; set; }// 昵称
    }

    public class UpdatePassword {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public struct UserBasic {
        public UserBasic(User user) {
            Nickname = user.Nickname ?? string.Empty;
        }
        public string Nickname { get; set; }
    }

    public struct UserPro {
        public UserPro(User user) {
            Phone = user.Phone;
            Name = user.Name;
            IdentityCard = user.IdentityCard;
            Nickname = user.Nickname;
        }
        public string Phone { get; set; }
        public string? Name { get; set; }
        public string? IdentityCard { get; set; }
        public string? Nickname { get; set; }
    }
}
