using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Shared.Cache
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringKey = "ConnectionStrings:Redis")
        {
            // Register ConnectionMultiplexer as Singleton
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Redis") 
                    ?? configuration[connectionStringKey] 
                    ?? "localhost:6379";
                
                var configurationOptions = ConfigurationOptions.Parse(connectionString);
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectRetry = 5;
                configurationOptions.ConnectTimeout = 10000;
                
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            // Register Redis Cache Service
            services.AddScoped<IRedisCacheService, RedisCacheService>();

            return services;
        }

        public static IServiceCollection AddRedisCacheWithHealthCheck(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringKey = "ConnectionStrings:Redis")
        {
            services.AddRedisCache(configuration, connectionStringKey);

            // Add health check
            services.AddHealthChecks()
                .AddRedis(
                    configuration.GetConnectionString("Redis") 
                    ?? configuration[connectionStringKey] 
                    ?? "localhost:6379",
                    name: "redis",
                    tags: new[] { "ready", "redis" });

            return services;
        }
    }
}