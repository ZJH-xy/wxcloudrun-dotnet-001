using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Dao.RepositoryInterface.Web;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.IdentityModel.Tokens;
using Senparc.CO2NET.HttpUtility;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;
using Senparc.Weixin.WxOpen.Entities;
using System.Net.Http.Headers;
using System.Security.Policy;

namespace aspnetapp.Controllers.Web {
    public class UserControllerWeb : Controller, IUserRepositoryWeb {

        private readonly MyDbContext _context;
        private readonly ILogger<UserControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public UserControllerWeb(MyDbContext context, ILogger<UserControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public async Task<List<User>> GetAllList() {
            return await _context.User.ToListAsync();
        }

        public async Task<User?> GetById(int id) {
            return await _context.User.FindAsync(id);
        }

        public async Task<List<User>> GetTablePage(int limit, int pageIndex) {
            return await _context.User
                .OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();
        }

        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(User).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateUser(User updatedUser) {
            _logger.LogInformation("正在启动ID为{UserId}的用户的更新过程", updatedUser.Id);

            if (await _context.User.Where(u => u.Id != updatedUser.Id && u.Phone == updatedUser.Phone).AnyAsync())
                return StatusCode(403, "与其它用户手机号重复！");

            var user = await _context.User.FindAsync(updatedUser.Id);
            if (user == null) {
                _logger.LogWarning("User with ID {UserId} not found", updatedUser.Id);
                return NotFound("User not found.");
            }

            // 更新用户属性
            user.Phone = updatedUser.Phone;
            //user.Password = updatedUser.Password;
            user.Name = updatedUser.Name;
            user.IdentityCard = updatedUser.IdentityCard;
            user.Nickname = updatedUser.Nickname;
            user.UpdatedAt = DateTime.Now;

            // 设置并发标记
            _context.Entry(user).Property("RowVersion").OriginalValue = updatedUser.RowVersion;

            try {
                _context.User.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID {UserId} updated successfully", updatedUser.Id);
                return Ok("User updated successfully.");

            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突 {UserId}", updatedUser.Id);
                return Conflict("Update failed due to concurrent changes.");

            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating user with ID {UserId}", updatedUser.Id);
                return StatusCode(500, "Error updating user.");

            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating user with ID {UserId}", updatedUser.Id);
                return StatusCode(500, "Unexpected error updating user.");
            }
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="name"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null) {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}", phone, name, nickname);

            // 构建查询的基础对象
            var query = _context.User.AsQueryable();

            // 根据传入的参数动态添加条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters", results.Count);

            return results;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="name"></param>
        /// <param name="nickname"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null, string sortField = "Id", string sortOrder = "asc") {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                                   phone, name, nickname, sortField, sortOrder);

            var query = _context.User.AsQueryable();

            // 过滤条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "phone" => sortOrder == "asc" ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone),
                "name" => sortOrder == "asc" ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
                "createdat" => sortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
            };

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters and sorting", results.Count);

            return results;
        }

        /// <summary>
        /// 更新用户图片信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="fileId"></param>
        /// <returns>返回更改行数，负数错误</returns>
        public async Task<int> PutImagePath(int userId, string fileId) {
            var user = await _context.User.SingleOrDefaultAsync(u => u.Id == userId);

            if (user is null)
                return -1;

            if (!user.IdentityCardPictures.IsNullOrEmpty()) {
                var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
                var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
                var envId = _wxSetting.Value.Env;

                List<string> fileid_list = new() {
                    user.IdentityCardPictures!
                };

                // 删除原图片
                var re = await TcbApi.BatchDeleteFileAsync(appId, envId, fileid_list);
                if (re.errcode != ReturnCode.请求成功) {
                    _logger.LogError("删除用户{UserId}原图片{ImageId}", (object)user.Id, (object)user.IdentityCardPictures!);
                }
            }

            // 更新图片路径
            user.IdentityCardPictures = fileId;
            try {
                _context.User.Update((User)user);
                await _context.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogError("保存用户{UserId}身份证图片路径", user);
                return -2;
            }

            return 0;
        }
    }
}
