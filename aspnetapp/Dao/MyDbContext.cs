namespace aspnetapp.Dao {
    public partial class MyDbContext : DbContext {
        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<Store> Store { get; set; } = null!;
        public DbSet<Vehicle> Vehicle { get; set; } = null!;
        public DbSet<Order> Order { get; set; } = null!;
        public DbSet<HomepageAd> HomepageAd { get; set; } = null!;
        public DbSet<StoreMenu> StoreMenus { get; set; } = null!;
        public DbSet<VehicleReplacementRecord> VehicleReplacementRecord { get; set; } = null!;
        public DbSet<UserFavoritesStore> UserFavoritesStore { get; set; } = null!;
        public DbSet<StoreAccount> StoreAccount { get; set; } = null!;
        public DbSet<RevenueStatistics> RevenueStatistic { get; set; } = null!;
        public DbSet<AdminAccount> AdminAccount { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // 配置数据库连接
            DatabaseConfig.ConfigureMySql(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // 设置默认字符集和排序规则
            modelBuilder.UseCollation("utf8_general_ci").HasCharSet("utf8");

            // 额外的模型配置
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}