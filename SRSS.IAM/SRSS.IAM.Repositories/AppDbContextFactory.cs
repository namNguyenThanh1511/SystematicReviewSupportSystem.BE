using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql; // Thêm dòng này
using Pgvector.EntityFrameworkCore; // Thêm dòng này

namespace SRSS.IAM.Repositories
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var apiProjectPath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "SRSS.IAM.API");

            var envPath = Path.GetFullPath(Path.Combine(apiProjectPath, ".env"));
            if (File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);
            }

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("SRSS_IAM_DB")
                ?? "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=SRSS.IAM";

            // TẠO DATA SOURCE BUILDER KÈM VECTOR CHO LÚC CHẠY MIGRATION
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();
            var dataSource = dataSourceBuilder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // SỬ DỤNG DATA SOURCE ĐÃ CÓ VECTOR VÀ BẬT UseVector() CHO EF CORE
            optionsBuilder.UseNpgsql(dataSource, o => o.UseVector());

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}