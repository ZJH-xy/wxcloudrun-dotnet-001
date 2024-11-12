using aspnetapp.Dao.RepositoryInterface.Web;

namespace aspnetapp.Controllers.Web {
    public class UserControllerWeb : Controller, IUserRepositoryWeb {

        private readonly MyDbContext _context;
        private readonly ILogger<UserControllerWeb> _logger;

        public UserControllerWeb(MyDbContext context, ILogger<UserControllerWeb> logger) {
            _context = context;
            _logger = logger;
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
            _logger.LogInformation("Starting update process for user with ID {UserId}", updatedUser.Id);

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
            user.IdentityCardPictures = updatedUser.IdentityCardPictures;
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
    }
}
