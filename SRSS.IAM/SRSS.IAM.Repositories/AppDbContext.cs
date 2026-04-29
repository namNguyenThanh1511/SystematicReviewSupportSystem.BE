using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SRSS.IAM.Repositories.Configurations;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		//"Fluent API Configurations" kết hợp với "Reflection".
		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.HasPostgresExtension("vector");
			builder.HasPostgresExtension("citext");
			builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
		}
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
		public DbSet<AuditLog> AuditLogs { get; set; } = default!;

		// Planning Phase
		public DbSet<SystematicReviewProject> SystematicReviewProjects { get; set; } = default!;
		public DbSet<ProjectMember> ProjectMembers { get; set; } = default!;
		public DbSet<ProjectMemberInvitation> ProjectMemberInvitations { get; set; } = default!;
		public DbSet<ResearchQuestion> ResearchQuestions { get; set; } = default!;
		public DbSet<QuestionType> QuestionTypes { get; set; } = default!;
		public DbSet<ProjectPicoc> ProjectPicocs { get; set; } = default!;
		public DbSet<PicocElement> PicocElements { get; set; } = default!;
		public DbSet<Population> Populations { get; set; } = default!;
		public DbSet<Intervention> Interventions { get; set; } = default!;
		public DbSet<Comparison> Comparisons { get; set; } = default!;
		public DbSet<Outcome> Outcomes { get; set; } = default!;
		public DbSet<Context> Contexts { get; set; } = default!;
		public DbSet<SearchSource> SearchSources { get; set; } = default!;
		public DbSet<SearchStrategy> SearchStrategies { get; set; } = default!;
		public DbSet<MasterSearchSources> MasterSearchSources { get; set; } = default!;
		public DbSet<StudySelectionCriteria> StudySelectionCriterias { get; set; } = default!;
		public DbSet<InclusionCriterion> InclusionCriteria { get; set; } = default!;
		public DbSet<ExclusionCriterion> ExclusionCriteria { get; set; } = default!;
		public DbSet<StudySelectionCriteriaAIResponse> StudySelectionCriteriaAIResponses { get; set; } = default!;
		public DbSet<ReviewNeed> ReviewNeeds { get; set; } = default!;
		public DbSet<ReviewObjective> ReviewObjectives { get; set; } = default!;
		public DbSet<CommissioningDocument> CommissioningDocuments { get; set; } = default!;
		public DbSet<QualityAssessmentStrategy> QualityAssessmentStrategies { get; set; } = default!;
		public DbSet<QualityChecklist> QualityChecklists { get; set; } = default!;
		public DbSet<QualityCriterion> QualityCriteria { get; set; } = default!;
		public DbSet<QualityAssessmentProcess> QualityAssessmentProcesses { get; set; } = default!;
		public DbSet<QualityAssessmentAssignment> QualityAssessmentAssignments { get; set; } = default!;
		public DbSet<QualityAssessmentDecision> QualityAssessmentDecisions { get; set; } = default!;
		public DbSet<QualityAssessmentDecisionItem> QualityAssessmentDecisionItems { get; set; } = default!;
		//public DbSet<DataExtractionStrategy> DataExtractionStrategies { get; set; } = default!;
		//public DbSet<DataExtractionForm> DataExtractionForms { get; set; } = default!;
		//public DbSet<DataItemDefinition> DataItemDefinitions { get; set; } = default!;
		public DbSet<ExtractionTemplate> ExtractionTemplates { get; set; } = default!;
		public DbSet<ExtractionSection> ExtractionSections { get; set; } = default!;
		public DbSet<ExtractionMatrixColumn> ExtractionMatrixColumns { get; set; } = default!;

		public DbSet<ExtractionField> ExtractionFields { get; set; } = default!;
		public DbSet<FieldOption> FieldOptions { get; set; } = default!;
		public DbSet<ExtractionPaperTask> ExtractionPaperTasks { get; set; } = default!;
		public DbSet<ExtractedDataValue> ExtractedDataValues { get; set; } = default!;
		public DbSet<ExtractionComment> ExtractionComments { get; set; } = default!;
		public DbSet<ExtractedDataAuditLog> ExtractedDataAuditLogs { get; set; } = default!;

		public DbSet<DataSynthesisStrategy> DataSynthesisStrategies { get; set; } = default!;
		public DbSet<ReviewProcess> ReviewProcesses { get; set; } = default!;
		public DbSet<IdentificationProcess> IdentificationProcesses { get; set; } = default!;
		public DbSet<DataExtractionProcess> DataExtractionProcesses { get; set; } = default!;

		public DbSet<FilterSetting> FilterSettings { get; set; } = default!;
		public DbSet<Paper> Papers { get; set; } = default!;
		public DbSet<ImportBatch> ImportBatches { get; set; } = default!;
		public DbSet<PrismaReport> PrismaReports { get; set; } = default!;
		public DbSet<PrismaFlowRecord> PrismaFlowRecords { get; set; } = default!;
		public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; } = default!;
		public DbSet<ChecklistSectionTemplate> ChecklistSectionTemplates { get; set; } = default!;
		public DbSet<ChecklistItemTemplate> ChecklistItemTemplates { get; set; } = default!;
		public DbSet<ReviewChecklist> ReviewChecklists { get; set; } = default!;
		public DbSet<ChecklistItemResponse> ChecklistItemResponses { get; set; } = default!;
		public DbSet<StudySelectionProcess> StudySelectionProcesses { get; set; } = default!;
		public DbSet<ScreeningDecision> ScreeningDecisions { get; set; } = default!;
		public DbSet<ScreeningResolution> ScreeningResolutions { get; set; } = default!;
		public DbSet<DeduplicationResult> DeduplicationResults { get; set; } = default!;
		public DbSet<IdentificationProcessPaper> IdentificationProcessPapers { get; set; } = default!;
		public DbSet<PaperAssignment> PaperAssignments { get; set; } = default!;
		public DbSet<StudySelectionProcessPaper> StudySelectionProcessPapers { get; set; } = default!;
		public DbSet<TitleAbstractScreening> TitleAbstractScreenings { get; set; } = default!;
		public DbSet<FullTextScreening> FullTextScreenings { get; set; } = default!;
		public DbSet<PaperPdf> PaperPdfs { get; set; } = default!;
		public DbSet<PaperSourceMetadata> PaperSourceMetadatas { get; set; } = default!;
		public DbSet<GrobidHeaderResult> GrobidHeaderResults { get; set; } = default!;
		public DbSet<CandidatePaper> CandidatePapers { get; set; } = default!;
		public DbSet<PaperCitation> PaperCitations { get; set; } = default!;
		public DbSet<PaperEmbedding> PaperEmbeddings { get; set; } = default!;
		public DbSet<StudySelectionAIResult> StudySelectionAIResults { get; set; } = default!;
		public DbSet<PaperFullText> PaperFullTexts { get; set; } = default!;
		public DbSet<PaperFullTextParsedSection> PaperFullTextParsedSections { get; set; } = default!;
		public DbSet<PaperFullTextParsedParagraph> PaperFullTextParsedParagraphs { get; set; } = default!;
		public DbSet<PaperFullTextChunk> PaperFullTextChunks { get; set; } = default!;
		public DbSet<PaperFullTextChunkEmbedding> PaperFullTextChunkEmbeddings { get; set; } = default!;
		public DbSet<ExclusionReasonLibrary> ExclusionReasonLibraries { get; set; } = default!;
		public DbSet<StudySelectionExclusionReason> StudySelectionExclusionReasons { get; set; } = default!;

		// Study Selection Checklist
		public DbSet<StudySelectionChecklistTemplate> StudySelectionChecklistTemplates { get; set; } = default!;
		public DbSet<StudySelectionChecklistTemplateSection> StudySelectionChecklistTemplateSections { get; set; } = default!;
		public DbSet<StudySelectionChecklistTemplateItem> StudySelectionChecklistTemplateItems { get; set; } = default!;
		public DbSet<StudySelectionChecklistSubmission> StudySelectionChecklistSubmissions { get; set; } = default!;
		public DbSet<StudySelectionChecklistSubmissionSectionAnswer> StudySelectionChecklistSubmissionSectionAnswers { get; set; } = default!;
		public DbSet<StudySelectionChecklistSubmissionItemAnswer> StudySelectionChecklistSubmissionItemAnswers { get; set; } = default!;

		// RAG pipeline
		public DbSet<PaperChunk> PaperChunks { get; set; } = default!;

		// Synthesis Phase
		public DbSet<SynthesisProcess> SynthesisProcesses { get; set; } = default!;
		public DbSet<SynthesisTheme> SynthesisThemes { get; set; } = default!;
		public DbSet<ThemeEvidence> ThemeEvidences { get; set; } = default!;
		public DbSet<ResearchQuestionFinding> ResearchQuestionFindings { get; set; } = default!;
	}
}
