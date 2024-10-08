namespace aspnetapp.Dao {
    public partial class MyDbContext : DbContext {
        public MyDbContext() { }

        public DbSet<User> User { get; set; }
        public DbSet<Store> Store { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<HomepageAd> homepageAds { get; set; }

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
