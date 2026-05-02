using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Cache;
using Shared.DependencyInjection;
using Shared.Middlewares;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.NotificationRepo;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.AuthService;
using SRSS.IAM.Services.Configurations;
using SRSS.IAM.Services.IdentificationService;
using SRSS.IAM.Services.JWTService;
using SRSS.IAM.Services.Mappers;
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
using SRSS.IAM.Services.CitationService;
using SRSS.IAM.Services.ProjectMemberInvitationService;
using SRSS.IAM.Services.AuditLogService;
using SRSS.IAM.Services.CandidatePaperService;
using System.Text;
using SRSS.IAM.API.Data;
using SRSS.IAM.Services.CoreGovernService;
using SRSS.IAM.Services.DataExtractionService;
using SRSS.IAM.Services.NotificationService;
using SRSS.IAM.Services.SupabaseService;
using SRSS.IAM.Services.GrobidClient;
using SRSS.IAM.Services.MetadataMergeService;
using SRSS.IAM.Services.EmbeddingService;
using SRSS.IAM.Services.ReferenceClassificationService;
using SRSS.IAM.Services.ReferenceMatchingService;
using SRSS.IAM.Services.OpenAlex;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using SRSS.IAM.Services.StudySelectionProcessPaperService;
using SRSS.IAM.Services.PaperEnrichmentService;
using SRSS.IAM.Services.GeminiService;
using SRSS.IAM.Services.StudySelectionAIService;
using SRSS.IAM.Services.ReferenceProcessingService;
using SRSS.IAM.Services.PaperFullTextService;
using SRSS.IAM.Services.ExclusionReasonLibraryService;
using SRSS.IAM.Services.StuSeExclusionCodeService;
using SRSS.IAM.Services.AdminMasterSourceService;
using SRSS.IAM.Services.Crossref;
using SRSS.IAM.Services.Parsers;
using SRSS.IAM.Services.DTOs.Crossref;
using SRSS.IAM.Services.PaperFullTextService.Parser;
using SRSS.IAM.Services.PaperFullTextService.Chunking;
using SRSS.IAM.Services.PaperFullTextService.Embedding;
using SRSS.IAM.Services.PaperFullTextService.Search;
using SRSS.IAM.Services.StudySelectionAIService.Retrieval;
using SRSS.IAM.Services.AiSetupService;
using SRSS.IAM.Services.OpenRouter;



using SRSS.IAM.Services.StudySelectionCriteriaService;
using SRSS.IAM.Services.RagService;
using SmartComponents.LocalEmbeddings;
using SRSS.IAM.Services.SynthesisExecutionService;
using SRSS.IAM.Services.ChecklistService;
using SRSS.IAM.Services.StudySelectionChecklists;
using SRSS.IAM.Services.Interceptors;

namespace SRSS.IAM.API.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddSignalR();
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IUserConnectionRepository, UserConnectionRepository>();


            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Planning Phase
            services.AddScoped<ICoreGovernService, CoreGovernService>();
            services.AddScoped<IResearchQuestionService, ResearchQuestionService>();
            services.AddScoped<ISearchStrategyService, SearchStrategyService>();
            services.AddScoped<ISelectionCriteriaService, SelectionCriteriaService>();
            services.AddScoped<IQualityAssessmentService, QualityAssessmentService>();
            services.AddScoped<IDataExtractionService, DataExtractionService>();
            services.AddScoped<IDataExtractionConductingService, DataExtractionConductingService>();
            services.AddScoped<ISynthesisService, SynthesisService>();
            services.AddScoped<IAiSetupService, AiSetupService>();
            services.AddScoped<ISearchStrategyAiService, SearchStrategyAiService>();





            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IIdentificationService, IdentificationService>();

            // Parser abstraction layer
            services.AddScoped<IRisParser, RisFileParser>();
            services.AddScoped<IDoiParser, CrossrefDoiParser>();
            services.AddScoped<IApiParser<CrossrefQueryParameters>, CrossrefApiParser>();
            services.AddScoped<ISystematicReviewProjectService, SystematicReviewProjectService>();
            services.AddScoped<IReviewProcessService, ReviewProcessService>();
            services.AddScoped<IPaperService, PaperService>();
            services.AddScoped<ICandidatePaperService, CandidatePaperService>();
            services.AddScoped<IPrismaReportService, PrismaReportService>();
            services.AddSingleton<IGeminiService, GeminiService>();
            services.AddScoped<ISelectionStatusService, SelectionStatusService>();
            services.AddScoped<IStudySelectionService, StudySelectionService>();
            services.AddScoped<IStuSeAIService, StuSeAIService>();
            services.AddScoped<IStudySelectionAIResultService, StudySelectionAIResultService>();
            services.AddScoped<IStudySelectionProcessPaperService, StudySelectionProcessPaperService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICitationService, CitationService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IProjectInvitationService, ProjectInvitationService>();
            services.AddScoped<IExclusionReasonLibraryService, ExclusionReasonLibraryService>();
            services.AddScoped<IStuSeExclusionCodeService, StuSeExclusionCodeService>();
            services.AddScoped<IMasterSearchSourceService, MasterSearchSourceService>();

            services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

            // GROBID integration
            services.Configure<GrobidOptions>(configuration.GetSection("Grobid"));
            services.AddHttpClient<IGrobidClient, GrobidClient>();
            services.AddScoped<IGrobidService, GrobidService>();
            services.AddScoped<IMetadataMergeService, MetadataMergeService>();
            services.AddScoped<IReferenceMatchingService, ReferenceMatchingService>();
            services.AddScoped<IReferenceClassificationService, ReferenceClassificationService>();
            services.AddScoped<IReferenceProcessingService, ReferenceProcessingService>();
            services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();

            // OpenRouter integration
            services.Configure<OpenRouterOptions>(configuration.GetSection(OpenRouterOptions.SectionName));
            services.AddHttpClient<IOpenRouterService, OpenRouterService>();

            // OpenAlex integration
            services.Configure<OpenAlexSettings>(configuration.GetSection(OpenAlexSettings.SectionName));
            services.AddHttpClient<IOpenAlexService, OpenAlexService>(client =>
            {
                client.BaseAddress = new Uri("https://api.openalex.org/");
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            // Crossref integration
            services.Configure<CrossrefSettings>(configuration.GetSection(CrossrefSettings.SectionName));
            services.AddHttpClient<ICrossrefService, CrossrefService>(client =>
            {
                var settings = configuration.GetSection(CrossrefSettings.SectionName).Get<CrossrefSettings>() ?? new CrossrefSettings();
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            // Paper enrichment (OpenAlex metadata)
            services.AddScoped<IPaperEnrichmentService, PaperEnrichmentService>();

            // Downstream-driven enrichment orchestrator & background worker
            services.AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<Guid>());
            services.AddScoped<IPaperEnrichmentOrchestrator, PaperEnrichmentOrchestrator>();
            services.AddHostedService<PaperEnrichmentBackgroundService>();

            // Reference processing background worker
            services.AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<ReferenceProcessingJob>());
            services.AddHostedService<ReferenceProcessingBackgroundService>();

            // Paper full-text extraction background worker
            services.AddSingleton<IPaperFullTextQueue, PaperFullTextQueue>();
            services.AddScoped<ITeiXmlParser, TeiXmlParser>();
            services.AddScoped<IPaperFullTextService, PaperFullTextService>();
            services.AddScoped<IPaperFullTextChunkingService, PaperFullTextChunkingService>();
            services.AddScoped<IPaperFullTextChunkEmbeddingService, PaperFullTextChunkEmbeddingService>();
            services.AddScoped<IPaperFullTextPreparationService, PaperFullTextPreparationService>();
            services.AddScoped<IPaperChunkSemanticSearchService, PaperChunkSemanticSearchService>();
            services.AddScoped<IStuSeProtocolRetrievalQueryBuilder, StuSeProtocolRetrievalQueryBuilder>();
            services.AddScoped<IStuSeProtocolChunkRetrievalService, StuSeProtocolChunkRetrievalService>();
            services.AddScoped<IStuSeFullTextAiEvaluationService, StuSeFullTextAiEvaluationService>();
            services.AddSingleton<IStuSeFullTextAiEvaluationQueue, StuSeFullTextAiEvaluationQueue>();
            services.AddHostedService<PaperFullTextBackgroundService>();
            services.AddHostedService<StuSeFullTextAiEvaluationBackgroundService>();

            // GROBID background worker
            services.AddSingleton<IGrobidProcessingQueue, GrobidProcessingQueue>();
            services.AddHostedService<GrobidBackgroundService>();

            // RAG pipeline — Local CPU embedding (Singleton: ONNX model loads once)
            services.AddSingleton<LocalEmbedder>();
            services.AddSingleton<ILocalEmbeddingService, LocalEmbeddingService>();
            services.AddSingleton<IRagIngestionQueue, RagIngestionQueue>();
            services.AddHostedService<RagIngestionBackgroundService>();
            services.AddScoped<IRagRetrievalService, RagRetrievalService>();

            // Synthesis Execution
            services.AddScoped<ISynthesisExecutionService, SynthesisExecutionService>();

            // Checklist
            services.AddScoped<IChecklistTemplateService, ChecklistTemplateService>();
            services.AddScoped<IReviewChecklistService, ReviewChecklistService>();
            services.AddSingleton<IChecklistAutoFillQueue, ChecklistAutoFillQueue>();
            services.AddHostedService<ChecklistAutoFillBackgroundService>();

            // Study Selection Checklist
            services.AddScoped<IStudySelectionChecklistService, StudySelectionChecklistService>();
            services.AddScoped<IStudySelectionChecklistSubmissionService, StudySelectionChecklistSubmissionService>();

            services.AddScoped<IStudySelectionCriteriaService, StudySelectionCriteriaService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<AuditInterceptor>();
        }

        public static void AddCorsPolicy(this IServiceCollection services, string policyName, IConfiguration configuration)
        {
            // Lấy danh sách từ config
            var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName, builder =>
                {
                    // Kiểm tra nếu là môi trường dev (không có origin nào được định nghĩa)
                    if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*"))
                    {
                        builder.SetIsOriginAllowed(origin => true) // Cho phép mọi Origin
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials(); // Quan trọng để không lỗi Preflight
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
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/notification"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
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
