using Microsoft.EntityFrameworkCore;
using aspnetapp.Dao.ContextBases;
using aspnetapp.Models;

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
            modelBuilder.Entity<User>().ToTable("Users");// 数据库表名
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class UserRepository : IUserRepository {
        private readonly UserContext _context;

        public UserRepository(UserContext context) {
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

        public async Task<User>? GetUserById(int id) {
            User user = await _context.Users.FindAsync(id);
            return user;
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
