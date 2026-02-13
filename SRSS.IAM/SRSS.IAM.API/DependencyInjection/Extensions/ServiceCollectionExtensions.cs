using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Cache;
using Shared.DependencyInjection;
using Shared.Middlewares;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.AuthService;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.JWTService;
using SRSS.IAM.Services.RefreshTokenService;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.SystematicReviewProjectService;
using SRSS.IAM.Services.ReviewProcessService;
using System.Text;
using SRSS.IAM.API.Data;

namespace SRSS.IAM.API.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddSignalR();
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IIdentificationService, IdentificationService>();
            services.AddScoped<ISystematicReviewProjectService, SystematicReviewProjectService>();
            services.AddScoped<IReviewProcessService, ReviewProcessService>();



        }

        public static void AddCorsPolicy(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin()
                                             .AllowAnyMethod()
                                             .AllowAnyHeader();
                                  });
            });
        }

        public static IServiceCollection ConfigureSwaggerForAuthentication(this IServiceCollection services)
        {
            services.AddSwaggerGenForAuthentication();
            return services;
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["secretKey"];

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidIssuer"],
                    ValidAudience = jwtSettings["ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }

        public static void ConfigureGlobalException(this IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<GlobalExceptionMiddleware>();
        }
        public async static void Seed(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                await DbInitializer.SeedAsync(scope.ServiceProvider);
            }
        }
    }
}
