using aspnetapp.Dao.RepositoryInterface.Web;

namespace aspnetapp.Controllers.Web {
    public class UserControllerWeb : Controller, IUserRepositoryWeb {

        private readonly MyDbContext _context;

        public UserControllerWeb(MyDbContext context) {
            _context = context;
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
        /// <param name="id"></param>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateUser(int id, User updatedUser) {
            var user = await _context.User.FindAsync(id);
            if (user == null) {
                return NotFound("User not found.");
            }

            // 更新用户属性
            user.Phone = updatedUser.Phone;
            user.Password = updatedUser.Password;
            user.Name = updatedUser.Name;
            user.IdentityCard = updatedUser.IdentityCard;
            user.IdentityCardPictures = updatedUser.IdentityCardPictures;
            user.Nickname = updatedUser.Nickname;
            user.UpdatedAt = DateTime.Now; // 更新修改时间

            try {
                await _context.SaveChangesAsync();
                return Ok("User updated successfully.");
            } catch (DbUpdateException) {
                return StatusCode(500, "Error updating user.");
            }
        }
    }
}
