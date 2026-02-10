using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Cache;
using Shared.Middlewares;

namespace Shared.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration config, string fileName)
        {
            // add generic database context 
            //services.AddDbContext<TContext>(option => option.UseSqlServer((config).GetConnectionString("SRSS"),
            //    sqlServerOption => sqlServerOption.EnableRetryOnFailure()));
            //configure Serilog logging 
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                outputTemplate: "Timestamp:yyyy-MM-dd HH:mm:ss.ff zzz [{Level:u3}] {message:lj}{NewLine}{Excepion}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
            //Add JWT Authentication Scheme
            JwtAuthenticationScheme.AddJwtAuthenticationScheme(services, config);



            //Use global exception handling middleware
            services.AddExceptionHandler<GlobalExceptionMiddleware>();
            return services;

        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {

            //Use JWT Blacklist Middleware
            app.UseMiddleware<JwtBlacklistMiddleware>();

            //Register middleware to block all outsiders API calls 
            // app.UseMiddleware<ListenToOnlyApiGateway>();

            return app;
        }

    }
}
