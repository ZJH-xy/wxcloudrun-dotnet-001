namespace aspnetapp.Controllers {
    public class UserController : IUserRepository {

        private readonly MyDbContext _context;

        public UserController(MyDbContext context) {
            _context = context;
        }

        public DbContext GetContext() {
            return _context;
        }

        public async Task<int> AddUser(User user) {
            await _context.User.AddAsync(user);
            return await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUser(int id) {
            User? user = await _context.User.SingleOrDefaultAsync(u => u.UserId == id);
            return user;
        }

        public async Task<User?> GetUserById(int id) {
            return await _context.User.SingleOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetUserByPhone(string phone) {
            return await _context.User.SingleOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<int> DeleteUserById(int id) {
            User? user = await _context.User.FindAsync(id);
            if (user != null) {
                _context.User.Remove(user);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateUser(User user) {
            user.UpdatedAt = DateTime.Now;
            _context.User.Update(user);
            return await _context.SaveChangesAsync();
        }
    }
}