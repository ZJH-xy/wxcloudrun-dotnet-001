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
}
