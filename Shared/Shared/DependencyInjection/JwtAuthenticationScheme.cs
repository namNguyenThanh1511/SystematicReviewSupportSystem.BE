using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

namespace Shared.DependencyInjection
{
    public static class JwtAuthenticationScheme
    {
        public static IServiceCollection AddJwtAuthenticationScheme(
            this IServiceCollection services,
            IConfiguration config)
        {
            var secretKey = Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!);
            var issuer = config["JwtSettings:ValidIssuer"];
            var audience = config["JwtSettings:ValidAudience"];
            var validateAudience = config.GetValue<bool>("JwtSettings:ValidateAudience");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = false;
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = validateAudience,
                        ValidAudience = validateAudience ? audience : null,

                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });

            return services;
        }

        public static IServiceCollection AddSwaggerGenForAuthentication(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                        "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                        "Example: \"Bearer [token]\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                });
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SRSS", Version = "v1" });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
                //**Main project's XML docs
                //var apiXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXml);
                //options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
            });
            return services;
        }

    }
}