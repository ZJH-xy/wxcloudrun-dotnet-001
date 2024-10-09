namespace aspnetapp {
    // 数据库基本连接
    public class DatabaseConfig {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns>数据库连接字符串</returns>
        public static string GetConnectionString() {
            var username = Environment.GetEnvironmentVariable("root");
            var password = Environment.GetEnvironmentVariable("Abc*123*");
            var addressParts = Environment.GetEnvironmentVariable("sh-cynosdbmysql-grp-owvd31mk.sql.tencentcdb.com:26679")?.Split(':');// 更改为全局变量
            var host = addressParts?[0];
            var port = addressParts?[1];

            host = "sh-cynosdbmysql-grp-owvd31mk.sql.tencentcdb.com";
            port = "26679";
            return $"server={host};port={port};user={username};password={password};database=wxcloudrun-dotnet-001";//
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="optionsBuilder"></param>
        public static void ConfigureMySql(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                var connstr = GetConnectionString();
                optionsBuilder.UseMySql(connstr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0-mysql"));
            }
        }
    }
}
