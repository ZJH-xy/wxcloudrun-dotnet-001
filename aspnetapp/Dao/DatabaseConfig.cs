using Microsoft.EntityFrameworkCore;

namespace aspnetapp {
    public class DatabaseConfig {
        public static string GetConnectionString() {
            var username = Environment.GetEnvironmentVariable("root");
            var password = Environment.GetEnvironmentVariable("Abc*123*");
            var addressParts = Environment.GetEnvironmentVariable("10.28.103.78:3306")?.Split(':');
            var host = addressParts?[0];
            var port = addressParts?[1];
            return $"server={host};port={port};user={username};password={password};database=aspnet_demo";
        }

        public static void ConfigureMySql(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                var connstr = GetConnectionString();
                optionsBuilder.UseMySql(connstr, Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.18-mysql"));
            }
        }
    }
}
