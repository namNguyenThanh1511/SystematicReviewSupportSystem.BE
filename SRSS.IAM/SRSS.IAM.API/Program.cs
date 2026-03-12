
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Cache;
using Shared.Middlewares;
using Shared.Models;
using SRSS.IAM.API.DependencyInjection.Extensions;
using SRSS.IAM.Repositories;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace SRSS.IAM.API
{
    public class Program
    {
        public static void Main(string[] args)
        {

			Env.Load();

			var builder = WebApplication.CreateBuilder(args);

			// Thêm Environment Variables vào Configuration
			builder.Configuration.AddEnvironmentVariables();

			var config = builder.Configuration;
            // Add services to the container.
            //DotNetEnv.Env.Load();

            builder.Services.AddControllers().AddXmlSerializerFormatters();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.ConfigureSwaggerForAuthentication();
            builder.Services.ConfigureJWT(config);
            builder.Services.ConfigureGlobalException();

			// Add CORS policy to allow all origins (for development/testing purposes)
			builder.Services.AddCorsPolicy("AllowAll");

			// Configure logging (Console + Debug)
			builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var environment = builder.Environment.EnvironmentName;
            builder.Logging.AddConsole();



            // Database connection
            var connectionString = $"Host={config["POSTGRES_HOST"]};" +
								 $"Port={config["POSTGRES_PORT"]};" +
								 $"Username={config["POSTGRES_USERNAME"]};" +
								 $"Password={config["POSTGRES_PASSWORD"]};" +
								 $"Database={config["POSTGRES_DATABASE"]}";
			builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

			// Redis connection

			var redisConnection = config["REDIS_CONNECTION"] ?? "localhost:6379";
			builder.Services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisConnection;
			});

			// 2. ĐĂNG KÝ THÊM IConnectionMultiplexer CHO RedisCacheService
			builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
			{
				// Khởi tạo connection tới Redis dựa trên chuỗi kết nối từ .env
				var configuration = StackExchange.Redis.ConfigurationOptions.Parse(redisConnection, true);
				return StackExchange.Redis.ConnectionMultiplexer.Connect(configuration);
			});

			builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

			builder.Services.AddApplicationServices(config);

			builder.Services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState
						.Where(x => x.Value.Errors.Count > 0)
						.SelectMany(x => x.Value.Errors.Select(e => new ApiError { Code = "INVALID_MODEL_STATE", Message = e.ErrorMessage }))
						.ToList();

					var response = new ApiResponse
					{
						IsSuccess = false,
						Message = "Dữ liệu không hợp lệ",
						Errors = errors
					};

					return new BadRequestObjectResult(response);
				};
			});


			var app = builder.Build();

            // 🔥 LOG MÔI TRƯỜNG VÀ CONNECTION INFO
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("🚀 Application starting in {Environment} environment", environment);
            logger.LogInformation("📦 PostgreSQL connection string: {Connection}", connectionString);
            logger.LogInformation("🔗 Redis connection: {Redis}", config.GetConnectionString("Redis"));

            app.UseMiddleware<JwtBlacklistMiddleware>();

            // Apply pending migrations automatically
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (db.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("🛠 Applying pending migrations...");
                    db.Database.Migrate();
                    logger.LogInformation("✅ Database migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("✅ No pending migrations found.");
                }
            }
            app.Seed();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    var publicBaseUrl = config["Swagger:PublicBaseUrl"];
                    swaggerDoc.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"/", Description = "Direct API Access" },
                    };
                });
            });

            app.UseSwaggerUI();
            app.UseCors("AllowAll");

			app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseExceptionHandler();

            //app.MapHub<ChatHub>("/hubs/chat"); // <--- Đường dẫn websocket
            app.MapControllers();

            app.Run();
        }
    }
}
