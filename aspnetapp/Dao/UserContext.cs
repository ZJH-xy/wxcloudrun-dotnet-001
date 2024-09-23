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

    public class UserRepository : UserRepositoryBase {
        private readonly UserContext _context;

        public UserRepository(UserContext context) {
            _context = context;
        }

        public override void AddUser(User user) {
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public override User? GetUserById(int id) {
            return _context.Users.Find(id);
        }

        public override IEnumerable<User> GetAllUsers() {
            return _context.Users.ToList();
        }

        public override void UpdateUser(User user) {
            var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null) {
                existingUser.Name = user.Name;
                existingUser.Phone = user.Phone;
                existingUser.UpdatedAt = DateTime.Now;

                _context.Users.Update(existingUser);
                _context.SaveChanges();
            }
        }

        public override void DeleteUser(int id) {
            var user = _context.Users.Find(id);
            if (user != null) {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
