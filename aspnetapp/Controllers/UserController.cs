using aspnetapp.Models;

namespace aspnetapp.Controllers {
    public class UserController : IUserRepository {

        private readonly UserContext _context;

        public UserController(UserContext context) {
            _context = context;
        }

        public async Task AddUser(User user) {
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            try {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            } catch (Exception ex) {
                throw new Exception("Error adding user", ex);// 记录异常日志
            }
        }

        public async Task<User?> GetUserById(int id) {
            return await _context.Users.FindAsync(id);
        }

        public IQueryable<User> GetAllUsers() {
            return _context.Users;
        }

        public async Task UpdateUser(User user) {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            /* var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null) {
                existingUser.Name = user.Name;
                existingUser.Phone = user.Phone;
                existingUser.UpdatedAt = DateTime.Now;

                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }*/
        }

        public async Task DeleteUser(int id) {
            var user = _context.Users.Find(id);
            if (user != null) {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
