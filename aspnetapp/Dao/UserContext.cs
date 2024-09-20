using aspnetapp.Models;
using Microsoft.EntityFrameworkCore;

namespace aspnetapp.Dao {
    public partial class UserContext : DbContext {
        public UserContext() { }

        public DbSet<User> Users { get; set; } = null!;

        public UserContext(DbContextOptions<UserContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            DatabaseConfig.ConfigureMySql(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.UseCollation("utf8_general_ci").HasCharSet("utf8");
            modelBuilder.Entity<User>().ToTable("Users");
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class UserRepository {
        private readonly UserContext _context;

        public UserRepository(UserContext context) {
            _context = context;
        }

        // 添加用户
        public void AddUser(User user) {
            user.createdAt = DateTime.Now;
            user.updatedAt = DateTime.Now;
            _context.Users.Add(user);
            _context.SaveChanges();
        }
        
        // 获取用户
        public User? GetUserById(int id) {
            return _context.Users.Find(id);
        }

        // 获取所有用户
        public IEnumerable<User> GetAllUsers() {
            return _context.Users.ToList();
        }

        // 更新用户
        public void UpdateUser(User user) {
            var existingUser = _context.Users.Find(user.Id);
            if (existingUser != null) {
                existingUser.Name = user.Name;
                existingUser.phone = user.phone;
                existingUser.Password = user.Password;
                existingUser.updatedAt = DateTime.Now;

                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }
        }

        // 删除用户
        public void DeleteUser(int id) {
            var user = _context.Users.Find(id);
            if (user != null) {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
