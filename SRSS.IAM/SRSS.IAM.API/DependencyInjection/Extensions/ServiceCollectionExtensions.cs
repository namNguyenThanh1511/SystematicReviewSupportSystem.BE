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
using SRSS.IAM.Services.Mappers;
using SRSS.IAM.Services.ProtocolService;
using SRSS.IAM.Services.QualityAssessmentService;
using SRSS.IAM.Services.RefreshTokenService;
using SRSS.IAM.Services.ResearchQuestionService;
using SRSS.IAM.Services.SearchStrategyService;
using SRSS.IAM.Services.SelectionCriteriaService;
using SRSS.IAM.Services.SynthesisService;
using SRSS.IAM.Services.UserService;
using SRSS.IAM.Services.SystematicReviewProjectService;
using SRSS.IAM.Services.ReviewProcessService;
using SRSS.IAM.Services.PaperService;
using SRSS.IAM.Services.PrismaReportService;
using SRSS.IAM.Services.SelectionStatusService;
using SRSS.IAM.Services.StudySelectionService;
using SRSS.IAM.Services.ProjectMemberInvitationService;
using SRSS.IAM.Services.TagService;
using System.Text;
using SRSS.IAM.API.Data;
using SRSS.IAM.Services.CoreGovernService;
using SRSS.IAM.Services.DataExtractionService;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.SupabaseService;

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

            // Planning Phase
            services.AddScoped<ICoreGovernService, CoreGovernService>();
            services.AddScoped<IProtocolService, ProtocolService>();
            services.AddScoped<IResearchQuestionService, ResearchQuestionService>();
            services.AddScoped<ISearchStrategyService, SearchStrategyService>();
            services.AddScoped<ISelectionCriteriaService, SelectionCriteriaService>();
            services.AddScoped<IQualityAssessmentService, QualityAssessmentService>();
            services.AddScoped<IDataExtractionService, DataExtractionService>();
            services.AddScoped<ISynthesisService, SynthesisService>();




            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IIdentificationService, IdentificationService>();
            services.AddScoped<ISystematicReviewProjectService, SystematicReviewProjectService>();
            services.AddScoped<IReviewProcessService, ReviewProcessService>();
            services.AddScoped<IPaperService, PaperService>();
            services.AddScoped<IPrismaReportService, PrismaReportService>();
            services.AddScoped<ISelectionStatusService, SelectionStatusService>();
            services.AddScoped<IStudySelectionService, StudySelectionService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
            services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();
            services.AddScoped<ITagService, TagService>();


		}

        public static void AddCorsPolicy(this IServiceCollection services, string policyName, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                ?? [];

            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName,
                                  builder =>
                                  {
                                      if (allowedOrigins.Length == 0)
                                      {
                                          builder.AllowAnyOrigin()
                                                 .AllowAnyMethod()
                                                 .AllowAnyHeader();
                                      }
                                      else
                                      {
                                          builder.WithOrigins(allowedOrigins)
                                                 .AllowAnyMethod()
                                                 .AllowAnyHeader()
                                                 .AllowCredentials();
                                      }
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
			var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
				?? throw new InvalidOperationException("JwtSettings section is missing in configuration");
			var secretKey = jwtSettings.SecretKey ?? throw new InvalidOperationException("JwtSettings:SecretKey is required");
			var validIssuer = jwtSettings.ValidIssuer;
			var validAudience = jwtSettings.ValidAudience;

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
                    ValidIssuer = validIssuer,
                    ValidAudience = validAudience,
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
