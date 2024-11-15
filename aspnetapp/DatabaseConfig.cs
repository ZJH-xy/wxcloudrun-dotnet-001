namespace aspnetapp {
    // 数据库基本连接
    public class DatabaseConfig {

        private readonly IConfiguration _configuration;

        public DatabaseConfig(IConfiguration configuration) {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        public string GetConnectionString() {
            // 优先从 appsettings.json 中读取数据库配置，如果没有则从环境变量获取
            var username = _configuration["DatabaseConfig:Username"] ?? Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root";
            var password = _configuration["DatabaseConfig:Password"] ?? Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "Abc*123*";
            var host = _configuration["DatabaseConfig:Host"] ?? Environment.GetEnvironmentVariable("DB_HOST") ?? "sh-cynosdbmysql-grp-o9fuosqg.sql.tencentcdb.com";
            var port = _configuration["DatabaseConfig:Port"] ?? Environment.GetEnvironmentVariable("DB_PORT") ?? "21713";
            var database = _configuration["DatabaseConfig:Database"] ?? Environment.GetEnvironmentVariable("DB_DATABASE") ?? "wxcloudrun-dotnet";

            // 获取环境变量中的数据库配置
            //var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root";
            //var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "Abc*123*";
            //var host = Environment.GetEnvironmentVariable("Host") ?? "";
            //var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "";
            //var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "wxcloudrun-dotnet";

            // 输出一些调试信息
            Console.WriteLine($"host: {host}, port: {port}, username: {username}");

            // 返回连接字符串
            return $"server={host};port={port};user={username};password={password};database={database}";
        }

        /// <summary>
        /// 配置 MySQL 数据库连接
        /// </summary>
        /// <param name="optionsBuilder"></param>
        public void ConfigureMySql(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                var connStr = GetConnectionString();
                optionsBuilder.UseMySql(connStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0-mysql"));
            }
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="optionsBuilder"></param>
        //public static void ConfigureMySql(DbContextOptionsBuilder optionsBuilder) {
        //    if (!optionsBuilder.IsConfigured) {
        //        var connStr = GetConnectionString();
        //        optionsBuilder.UseMySql(connStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0-mysql"));
        //    }
        //}
    }
}