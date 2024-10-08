namespace aspnetapp.Dao {
    public partial class MyDbContext : DbContext {
        public MyDbContext() { }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<Store> Store { get; set; } = null!;
        public DbSet<Vehicle> Vehicle { get; set; } = null!;
        public DbSet<Order> Order { get; set; } = null!;
        public DbSet<HomepageAd> homepageAds { get; set; } = null!;

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

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
