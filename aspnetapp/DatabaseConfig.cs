namespace aspnetapp {
    // 数据库基本连接
    public class DatabaseConfig {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        public static string GetConnectionString() {
            // 获取环境变量中的数据库配置
            var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "Abc*123*";
            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "sh-cynosdbmysql-grp-owvd31mk.sql.tencentcdb.com";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "26679";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "wxcloudrun-dotnet-001";

            // 输出一些调试信息
            Console.WriteLine($"host: {host}, port: {port}, username: {username}");

            // 返回连接字符串
            return $"server={host};port={port};user={username};password={password};database={database}";
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="optionsBuilder"></param>
        public static void ConfigureMySql(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                var connStr = GetConnectionString();
                optionsBuilder.UseMySql(connStr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0-mysql"));
            }
        }
    }
}