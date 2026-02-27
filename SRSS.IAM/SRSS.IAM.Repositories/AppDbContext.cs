using SRSS.IAM.Repositories.Configurations;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		//"Fluent API Configurations" kết hợp với "Reflection".
		protected override void OnModelCreating(ModelBuilder builder)
			   => builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
		/*
         Mục đích của việc sử dụng Assembly ở đây là:
            Tự động hóa cấu hình (Automation): Thay vì phải viết thủ công từng dòng builder.Entity<MyEntity>().Has... cho mọi thực thể (entity) trong dự án, bạn có thể tạo các lớp cấu hình riêng biệt (implementing IEntityTypeConfiguration<T>).
            Khám phá (Discovery/Scanning): Phương thức ApplyConfigurationsFromAssembly yêu cầu một đối tượng Assembly để EF Core có thể "quét" (scan) qua toàn bộ tập hợp đó, tìm kiếm tất cả các lớp cấu hình đã được định nghĩa, và áp dụng chúng một cách tự động vào ModelBuilder.
            Tổ chức mã nguồn (Code Organization): Bằng cách này, bạn giữ logic cấu hình cơ sở dữ liệu (ví dụ: tên bảng, ràng buộc khóa ngoại, kiểu dữ liệu) tách biệt khỏi chính các lớp thực thể (entities) hoặc DbContext, giúp mã nguồn sạch sẽ và dễ quản lý hơn.
                Tóm lại:
                    AssemblyReference.Assembly đóng vai trò là điểm khởi đầu để EF Core biết nơi nào để tìm kiếm các quy tắc cấu hình cơ sở dữ liệu mà bạn đã định nghĩa ở những tệp khác trong cùng một dự án (hoặc một dự án phụ thuộc khác).
         */

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
		}

		public DbSet<User> Users { get; set; } = default!;
		public DbSet<Notification> Notifications { get; set; } = default!;

		// Planning Phase
		public DbSet<SystematicReviewProject> SystematicReviewProjects { get; set; } = default!;
		public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
		public DbSet<ProjectMemberInvitation> ProjectMemberInvitations { get; set; } = default!;
		public DbSet<ReviewProtocol> ReviewProtocols { get; set; } = default!;
		public DbSet<ProtocolVersion> ProtocolVersions { get; set; } = default!;
		public DbSet<ResearchQuestion> ResearchQuestions { get; set; } = default!;
		public DbSet<QuestionType> QuestionTypes { get; set; } = default!;
		public DbSet<PicocElement> PicocElements { get; set; } = default!;
		public DbSet<Population> Populations { get; set; } = default!;
		public DbSet<Intervention> Interventions { get; set; } = default!;
		public DbSet<Comparison> Comparisons { get; set; } = default!;
		public DbSet<Outcome> Outcomes { get; set; } = default!;
		public DbSet<Context> Contexts { get; set; } = default!;
		public DbSet<SearchStrategy> SearchStrategies { get; set; } = default!;
		public DbSet<SearchString> SearchStrings { get; set; } = default!;
		public DbSet<SearchTerm> SearchTerms { get; set; } = default!;
		public DbSet<SearchStringTerm> SearchStringTerms { get; set; } = default!;
		public DbSet<SearchSource> SearchSources { get; set; } = default!;
		public DbSet<DigitalLibrary> DigitalLibraries { get; set; } = default!;
		public DbSet<Journal> Journals { get; set; } = default!;
		public DbSet<BibliographicDatabase> BibliographicDatabases { get; set; } = default!;
		public DbSet<ConferenceProceeding> ConferenceProceedings { get; set; } = default!;
		public DbSet<StudySelectionCriteria> StudySelectionCriterias { get; set; } = default!;
		public DbSet<InclusionCriterion> InclusionCriteria { get; set; } = default!;
		public DbSet<ExclusionCriterion> ExclusionCriteria { get; set; } = default!;
		public DbSet<ProtocolReviewer> ProtocolReviewers { get; set; } = default!;
		public DbSet<ProtocolEvaluation> ProtocolEvaluations { get; set; } = default!;
		public DbSet<ReviewNeed> ReviewNeeds { get; set; } = default!;
		public DbSet<ReviewObjective> ReviewObjectives { get; set; } = default!;
		public DbSet<CommissioningDocument> CommissioningDocuments { get; set; } = default!;
		public DbSet<StudySelectionProcedure> StudySelectionProcedures { get; set; } = default!;
		public DbSet<QualityAssessmentStrategy> QualityAssessmentStrategies { get; set; } = default!;
		public DbSet<QualityChecklist> QualityChecklists { get; set; } = default!;
		public DbSet<QualityCriterion> QualityCriteria { get; set; } = default!;
		public DbSet<DataExtractionStrategy> DataExtractionStrategies { get; set; } = default!;
		public DbSet<DataExtractionForm> DataExtractionForms { get; set; } = default!;
		public DbSet<DataItemDefinition> DataItemDefinitions { get; set; } = default!;
		public DbSet<DataSynthesisStrategy> DataSynthesisStrategies { get; set; } = default!;
		public DbSet<DisseminationStrategy> DisseminationStrategies { get; set; } = default!;
		public DbSet<ProjectTimetable> ProjectTimetables { get; set; } = default!;
		public DbSet<ReviewProcess> ReviewProcesses { get; set; } = default!;
		public DbSet<IdentificationProcess> IdentificationProcesses { get; set; } = default!;
		public DbSet<SearchExecution> SearchExecutions { get; set; } = default!;
		public DbSet<Paper> Papers { get; set; } = default!;
		public DbSet<ImportBatch> ImportBatches { get; set; } = default!;
		public DbSet<PrismaReport> PrismaReports { get; set; } = default!;
		public DbSet<PrismaFlowRecord> PrismaFlowRecords { get; set; } = default!;
		public DbSet<StudySelectionProcess> StudySelectionProcesses { get; set; } = default!;
		public DbSet<ScreeningDecision> ScreeningDecisions { get; set; } = default!;
		public DbSet<ScreeningResolution> ScreeningResolutions { get; set; } = default!;
		public DbSet<DeduplicationResult> DeduplicationResults { get; set; } = default!;
	}
}
