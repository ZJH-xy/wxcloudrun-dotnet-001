using Senparc.CO2NET.Extensions;

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
            // 获取当前环境
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // 根据环境选择不同的配置
            string username, password, host, port, database;

            if (environment == "Development") {
                // 开发环境使用 appsettings.json 中的配置或默认值
                username = _configuration["DatabaseConfig:Username"] ?? "root";
                password = _configuration["DatabaseConfig:Password"] ?? "Abc*123*";
                host = "sh-cynosdbmysql-grp-o9fuosqg.sql.tencentcdb.com";
                port = "21713";
                database = _configuration["DatabaseConfig:Database"] ?? "wxcloudrun-dotnet";
            } else {
                // 生产环境或其他环境使用环境变量
                username = _configuration["DatabaseConfig:Username"] ?? "root";
                password = _configuration["DatabaseConfig:Password"] ?? "Abc*123*";
                host = _configuration["DatabaseConfig:Host"] ?? "";
                port = _configuration["DatabaseConfig:Port"] ?? "";
                database = _configuration["DatabaseConfig:Database"] ?? "wxcloudrun-dotnet";
            }

            if (host.IsNullOrEmpty() || port.IsNullOrEmpty())
                throw new Exception("未配置数据库连接");

            // 输出调试信息
            Console.WriteLine($"Environment: {environment}");
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