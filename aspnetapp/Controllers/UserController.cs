namespace aspnetapp.Controllers {
    public class UserController : IUserRepository {

        private readonly MyDbContext _context;

        public UserController(MyDbContext context) {
            _context = context;
        }

        public async Task<int> AddUser(User user) {
            await _context.User.AddAsync(user);
            return await _context.SaveChangesAsync();
        }
        public async Task<User?> GetUser(int id) {
            User? user = await _context.User.SingleOrDefaultAsync(u => u.UserId == id);
            return user;
        }

        public async Task<UserBasic?> GetUserById(int id) {
            User? user = await _context.User.SingleOrDefaultAsync(u => u.UserId == id);
            return user is null ? null : new UserBasic(user);
        }

        public async Task<UserBasic?> GetUserByPhone(string phone) {
            User? user = await _context.User.SingleOrDefaultAsync(u => u.Phone == phone);
            return user is null ? null : new UserBasic(user);
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

        // 基础用户信息
        public struct UserBasic {
            public UserBasic(User user) {
                Id = user.UserId;
                Nickname = user.Nickname ?? string.Empty;
            }
            public int Id;
            public string Nickname;
        }

        public struct UserPro {
            public UserPro(User user) {
                UserId = user.UserId;
                Phone = user.Phone;
                Name = user.Name;
                IdentityCard = user.IdentityCard;
                Nickname = user.Nickname;
            }
            public int UserId;
            public string Phone;
            public string? Name;
            public string? IdentityCard;
            public string? Nickname;
        }
    }
}