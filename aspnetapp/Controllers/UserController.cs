namespace aspnetapp.Controllers
{
    public class UserController : IUserRepository {

        private readonly MyDbContext _context;

        public UserController(MyDbContext context) {
            _context = context;
        }

        public async Task<int> AddUser(User user) {
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            try {
                await _context.User.AddAsync(user);
                return await _context.SaveChangesAsync();
            } catch (Exception ex) {
                throw new Exception("Error adding user", ex);// 记录异常日志
            }
        }

        public async Task<Object?> GetUserById(int id) {
            return await _context.User.FindAsync(id);
        }

        public async Task<int> UpdateUser(User user) {
            user.UpdatedAt = DateTime.Now;
            _context.User.Update(user);
            return await _context.SaveChangesAsync();
            /* var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null) {
                existingUser.Name = user.Name;
                existingUser.Phone = user.Phone;
                existingUser.UpdatedAt = DateTime.Now;

                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }*/
        }

        public async Task<int> DeleteUserById(int id) {
            throw new NotImplementedException();
            //Object? user = await GetUserById(id);
            //if (user != null) {
            //    _context.User.Remove(user);
            //}
            //return await _context.SaveChangesAsync();
        }

        public Task<object?> GetUserByPhone(int phone) {
            throw new NotImplementedException();
        }
    }
}
