public partial class MyDbContext : DbContext {
    private readonly DatabaseConfig _databaseConfig;

    // 无参数构造函数（一般用于工具，如迁移）
    public MyDbContext() { }

    // 接收 DbContextOptions 和 DatabaseConfig 的构造函数
    public MyDbContext(DbContextOptions<MyDbContext> options, DatabaseConfig databaseConfig) : base(options) {
        _databaseConfig = databaseConfig;
    }

    // 数据库表集合
    public DbSet<User> User { get; set; } = null!;
    public DbSet<Store> Store { get; set; } = null!;
    public DbSet<Vehicle> Vehicle { get; set; } = null!;
    public DbSet<Order> Order { get; set; } = null!;
    public DbSet<SupplementaryOrders> SupplementaryOrders { get; set; } = null!;
    public DbSet<HomepageAd> HomepageAd { get; set; } = null!;
    public DbSet<StoreMenu> StoreMenus { get; set; } = null!;
    public DbSet<VehicleReplacementRecord> VehicleReplacementRecord { get; set; } = null!;
    public DbSet<UserFavoritesStore> UserFavoritesStore { get; set; } = null!;
    public DbSet<StoreAccount> StoreAccount { get; set; } = null!;
    public DbSet<RevenueStatistics> RevenueStatistic { get; set; } = null!;
    public DbSet<AdminAccount> AdminAccount { get; set; } = null!;
    public DbSet<RefundOrder> RefundOrder { get; set; } = null!;
    public DbSet<Notice> Notice { get; set; } = null!;
    public DbSet<Advertisement> Advertisement { get; set; } = null!;
    public DbSet<ReviewMerchantAddress> ReviewMerchantAddress { get; set; } = null!;
    public DbSet<OrderEvaluate> OrderEvaluate { get; set; } = null!;
    public DbSet<OrderLog> OrderLog { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        // 如果未配置，则使用 _databaseConfig 配置
        if (!optionsBuilder.IsConfigured && _databaseConfig != null) {
            _databaseConfig.ConfigureMySql(optionsBuilder);
        }
    }
}
