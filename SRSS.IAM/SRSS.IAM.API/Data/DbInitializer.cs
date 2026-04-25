using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using System.Text.Json;

namespace SRSS.IAM.API.Data
{
    public static class DbInitializer
    {
        // ── Project IDs ──────────────────────────────────────────────────
        private static readonly Guid HarProjectId = Guid.Parse("85e964d2-aa46-40a1-b305-1fad7f499641");
        private static readonly Guid ScientificLiteracyProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // ── Review Process IDs ───────────────────────────────────────────
        private static readonly Guid HarReviewProcessId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // ── Identification Process IDs ───────────────────────────────────
        private static readonly Guid HarIdentificationProcessId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // ── Study Selection Process IDs ─────────────────────────────────
        private static readonly Guid HarStudySelectionProcessId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // ── Quality Assessment Process IDs ──────────────────────────────
        private static readonly Guid HarQualityAssessmentProcessId = Guid.Parse("55555555-5555-5555-5555-555555555555");


        // ── Data Extraction Process IDs ─────────────────────────────────
        private static readonly Guid HarDataExtractionProcessId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // ── Synthesis Process IDs ───────────────────────────────────────
        private static readonly Guid HarSynthesisProcessId = Guid.Parse("66666666-5555-5555-5555-555555555555");

        // ── Search Execution IDs ─────────────────────────────────────────
        private static readonly Guid ScopusSearchExecutionId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // ── Import Batch IDs ─────────────────────────────────────────────
        private static readonly Guid ScopusImportBatchId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // ── Paper IDs ────────────────────────────────────────────────────
        private static readonly Guid Paper1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid Paper2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
        private static readonly Guid Paper3Id = Guid.Parse("88888888-8888-8888-8888-888888888888");

        // ── Screening Resolution IDs ─────────────────────────────────────
        private static readonly Guid ScreeningRes1Id = Guid.Parse("d1111111-2222-3333-4444-555555555555");
        private static readonly Guid ScreeningRes2Id = Guid.Parse("d2222222-2222-3333-4444-555555555555");
        private static readonly Guid ScreeningRes3Id = Guid.Parse("d3333333-2222-3333-4444-555555555555");

        // ── Core Governance IDs ──────────────────────────────────────────
        private static readonly Guid ReviewNeed1Id = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        private static readonly Guid ReviewNeed2Id = Guid.Parse("a2222222-2222-2222-2222-222222222222");
        private static readonly Guid CommissioningDocumentId = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        private static readonly Guid ReviewObjective1Id = Guid.Parse("c1111111-1111-1111-1111-111111111111");
        private static readonly Guid ReviewObjective2Id = Guid.Parse("c2222222-2222-2222-2222-222222222222");
        private static readonly Guid QuestionTypePicocId = Guid.Parse("d1111111-1111-1111-1111-111111111111");
        private static readonly Guid QuestionTypePicoId = Guid.Parse("d2222222-2222-2222-2222-222222222222");
        private static readonly Guid QuestionTypeGqmId = Guid.Parse("d3333333-3333-3333-3333-333333333333");
        private static readonly Guid ResearchQuestion1Id = Guid.Parse("e1111111-1111-1111-1111-111111111111");
        private static readonly Guid ResearchQuestion2Id = Guid.Parse("e2222222-2222-2222-2222-222222222222");

        // ── PICOC Element IDs for RQ1 ────────────────────────────────────
        private static readonly Guid Rq1PopulationId = Guid.Parse("f1111111-1111-1111-1111-111111111111");
        private static readonly Guid Rq1InterventionId = Guid.Parse("f1222222-2222-2222-2222-222222222222");
        private static readonly Guid Rq1ComparisonId = Guid.Parse("f1333333-3333-3333-3333-333333333333");
        private static readonly Guid Rq1OutcomeId = Guid.Parse("f1444444-4444-4444-4444-444444444444");
        private static readonly Guid Rq1ContextId = Guid.Parse("f1555555-5555-5555-5555-555555555555");

        // ── PICOC Detail IDs for RQ1 ─────────────────────────────────────
        private static readonly Guid Rq1PopulationDetailId = Guid.Parse("f1611111-1111-1111-1111-111111111111");
        private static readonly Guid Rq1InterventionDetailId = Guid.Parse("f1622222-2222-2222-2222-222222222222");
        private static readonly Guid Rq1ComparisonDetailId = Guid.Parse("f1633333-3333-3333-3333-333333333333");
        private static readonly Guid Rq1OutcomeDetailId = Guid.Parse("f1644444-4444-4444-4444-444444444444");
        private static readonly Guid Rq1ContextDetailId = Guid.Parse("f1655555-5555-5555-5555-555555555555");

        // ── PICOC Element IDs for RQ2 ────────────────────────────────────
        private static readonly Guid Rq2PopulationId = Guid.Parse("f2111111-1111-1111-1111-111111111111");
        private static readonly Guid Rq2InterventionId = Guid.Parse("f2222222-2222-2222-2222-222222222222");
        private static readonly Guid Rq2ComparisonId = Guid.Parse("f2333333-3333-3333-3333-333333333333");
        private static readonly Guid Rq2OutcomeId = Guid.Parse("f2444444-4444-4444-4444-444444444444");
        private static readonly Guid Rq2ContextId = Guid.Parse("f2555555-5555-5555-5555-555555555555");

        // ── PICOC Detail IDs for RQ2 ─────────────────────────────────────
        private static readonly Guid Rq2PopulationDetailId = Guid.Parse("f2611111-1111-1111-1111-111111111111");
        private static readonly Guid Rq2InterventionDetailId = Guid.Parse("f2622222-2222-2222-2222-222222222222");
        private static readonly Guid Rq2ComparisonDetailId = Guid.Parse("f2633333-3333-3333-3333-333333333333");
        private static readonly Guid Rq2OutcomeDetailId = Guid.Parse("f2644444-4444-4444-4444-444444444444");
        private static readonly Guid Rq2ContextDetailId = Guid.Parse("f2655555-5555-5555-5555-555555555555");

        // ── User IDs ─────────────────────────────────────────────────────
        private static readonly Guid AdminUserId = Guid.Parse("aa111111-1111-1111-1111-111111111111");
        private static readonly Guid ClientUserId = Guid.Parse("aa222222-2222-2222-2222-222222222222");



        // ── Search Source IDs ────────────────────────────────────────────
        private static readonly Guid SearchSource1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid SearchSource2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid SearchSource3Id = Guid.Parse("22333333-3333-3333-3333-333333333333");

        // ── Study Selection Criteria IDs ─────────────────────────────────
        private static readonly Guid SelectionCriteria1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid InclusionCriterion1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid InclusionCriterion2Id = Guid.Parse("11222222-2222-2222-2222-222222222222");
        private static readonly Guid ExclusionCriterion1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ExclusionCriterion2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");



        // ── Quality Assessment IDs ───────────────────────────────────────
        private static readonly Guid QualityStrategy1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid QualityChecklist1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid QualityCriterion1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid QualityCriterion2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // ── Data Extraction IDs ──────────────────────────────────────────
        private static readonly Guid ExtractionStrategy1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ExtractionForm1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid DataItem1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid DataItem2Id = Guid.Parse("11222222-2222-2222-2222-222222222222");

        // ── Data Extraction Template IDs (PICOC SE Template) ─────────────
        private static readonly Guid HarExtractionTemplateId = Guid.Parse("ee000000-0000-0000-0000-000000000001");

        // Sections
        private static readonly Guid EtSec1IdentificationId = Guid.Parse("ee000000-0001-0000-0000-000000000001");
        private static readonly Guid EtSec2ContextId = Guid.Parse("ee000000-0001-0000-0000-000000000002");
        private static readonly Guid EtSec3ContextId = Guid.Parse("ee000000-0001-0000-0000-000000000003");

        private static readonly Guid EtSec4MethodologyId = Guid.Parse("ee000000-0001-0000-0000-000000000004");
        private static readonly Guid EtSec5InterventionsId = Guid.Parse("ee000000-0001-0000-0000-000000000005");
        private static readonly Guid EtSec6MetricsId = Guid.Parse("ee000000-0001-0000-0000-000000000006");

        // ── Section 1: Identification Fields ─────────────────────────────
        private static readonly Guid EtF1StudyIdId = Guid.Parse("ee000000-0002-0000-0000-000000000001");
        private static readonly Guid EtF1TitleId = Guid.Parse("ee000000-0002-0000-0000-000000000002");
        private static readonly Guid EtF1AuthorsId = Guid.Parse("ee000000-0002-0000-0000-000000000003");
        private static readonly Guid EtF1YearId = Guid.Parse("ee000000-0002-0000-0000-000000000004");
        private static readonly Guid EtF1PublicationTypeId = Guid.Parse("ee000000-0002-0000-0000-000000000005");
        private static readonly Guid EtF1VenueId = Guid.Parse("ee000000-0002-0000-0000-000000000006");
        private static readonly Guid EtF1DoiId = Guid.Parse("ee000000-0002-0000-0000-000000000007");
        private static readonly Guid EtF1CitationsId = Guid.Parse("ee000000-0002-0000-0000-000000000008");
        private static readonly Guid EtF1ContextId = Guid.Parse("ee000000-0002-0000-0000-000000000009");
        private static readonly Guid EtF1InterventionId = Guid.Parse("ee000000-0002-0000-0000-000000000010");
        private static readonly Guid EtF1ComparisonId = Guid.Parse("ee000000-0002-0000-0000-000000000011");
        private static readonly Guid EtF1OutcomeId = Guid.Parse("ee000000-0002-0000-0000-000000000012");

        // Options for Publication Type
        private static readonly Guid EtOptPubJournalId = Guid.Parse("ee000000-0003-0000-0000-000000000001");
        private static readonly Guid EtOptPubConferenceId = Guid.Parse("ee000000-0003-0000-0000-000000000002");
        private static readonly Guid EtOptPubWorkshopId = Guid.Parse("ee000000-0003-0000-0000-000000000003");
        private static readonly Guid EtOptPubThesisId = Guid.Parse("ee000000-0003-0000-0000-000000000004");
        private static readonly Guid EtOptPubTechReportId = Guid.Parse("ee000000-0003-0000-0000-000000000005");
        private static readonly Guid EtOptPubOtherId = Guid.Parse("ee000000-0003-0000-0000-000000000006");

        // ── Section 2: Study Context Fields ──────────────────────────────
        private static readonly Guid EtF2PopulationId = Guid.Parse("ee000000-0002-0001-0000-000000000001");
        private static readonly Guid EtF2DomainId = Guid.Parse("ee000000-0002-0001-0000-000000000002");
        private static readonly Guid EtF2ContextTypeId = Guid.Parse("ee000000-0002-0001-0000-000000000003");
        private static readonly Guid EtF2CountriesId = Guid.Parse("ee000000-0002-0001-0000-000000000004");
        private static readonly Guid EtF2IndustrySizeId = Guid.Parse("ee000000-0002-0001-0000-000000000005");
        private static readonly Guid EtF2EthicsId = Guid.Parse("ee000000-0002-0001-0000-000000000006");

        // Section 3 RQ Driven
        private static readonly Guid EtF3PopulationId_RQ2 = Guid.Parse("ee000000-0002-0001-0000-000000000011");
        private static readonly Guid EtF3DomainId_RQ2 = Guid.Parse("ee000000-0002-0001-0000-000000000012");

        // Options for Context Type
        private static readonly Guid EtOptCtxIndustrialId = Guid.Parse("ee000000-0004-0000-0000-000000000001");
        private static readonly Guid EtOptCtxAcademicId = Guid.Parse("ee000000-0004-0000-0000-000000000002");
        private static readonly Guid EtOptCtxLabId = Guid.Parse("ee000000-0004-0000-0000-000000000003");
        private static readonly Guid EtOptCtxOnlineId = Guid.Parse("ee000000-0004-0000-0000-000000000004");
        private static readonly Guid EtOptCtxMixedId = Guid.Parse("ee000000-0004-0000-0000-000000000005");

        // Options for Industry Size
        private static readonly Guid EtOptSizeSmeId = Guid.Parse("ee000000-0005-0000-0000-000000000001");
        private static readonly Guid EtOptSizeLargeId = Guid.Parse("ee000000-0005-0000-0000-000000000002");
        private static readonly Guid EtOptSizeMixedId = Guid.Parse("ee000000-0005-0000-0000-000000000003");
        private static readonly Guid EtOptSizeNaId = Guid.Parse("ee000000-0005-0000-0000-000000000004");

        // ── Section 3: Research Methodology Fields ────────────────────────
        private static readonly Guid EtF3ResearchTypeId = Guid.Parse("ee000000-0002-0002-0000-000000000001");
        private static readonly Guid EtF3ResearchMethodId = Guid.Parse("ee000000-0002-0002-0000-000000000002");
        private static readonly Guid EtF3StudyDesignId = Guid.Parse("ee000000-0002-0002-0000-000000000003");
        private static readonly Guid EtF3SampleSizeId = Guid.Parse("ee000000-0002-0002-0000-000000000004");
        private static readonly Guid EtF3SamplingId = Guid.Parse("ee000000-0002-0002-0000-000000000005");
        private static readonly Guid EtF3DataCollectionId = Guid.Parse("ee000000-0002-0002-0000-000000000006");
        private static readonly Guid EtF3ThreatsValidityId = Guid.Parse("ee000000-0002-0002-0000-000000000007");
        private static readonly Guid EtF3ReplicationId = Guid.Parse("ee000000-0002-0002-0000-000000000008");

        // Options for Research Type
        private static readonly Guid EtOptRtEmpiricalId = Guid.Parse("ee000000-0006-0000-0000-000000000001");
        private static readonly Guid EtOptRtExperimentId = Guid.Parse("ee000000-0006-0000-0000-000000000002");
        private static readonly Guid EtOptRtCaseStudyId = Guid.Parse("ee000000-0006-0000-0000-000000000003");
        private static readonly Guid EtOptRtSurveyId = Guid.Parse("ee000000-0006-0000-0000-000000000004");
        private static readonly Guid EtOptRtSimulationId = Guid.Parse("ee000000-0006-0000-0000-000000000005");
        private static readonly Guid EtOptRtReviewId = Guid.Parse("ee000000-0006-0000-0000-000000000006");
        private static readonly Guid EtOptRtOtherId = Guid.Parse("ee000000-0006-0000-0000-000000000007");

        // Options for Sampling
        private static readonly Guid EtOptSampRandomId = Guid.Parse("ee000000-0007-0000-0000-000000000001");
        private static readonly Guid EtOptSampConvenienceId = Guid.Parse("ee000000-0007-0000-0000-000000000002");
        private static readonly Guid EtOptSampPurposiveId = Guid.Parse("ee000000-0007-0000-0000-000000000003");
        private static readonly Guid EtOptSampNaId = Guid.Parse("ee000000-0007-0000-0000-000000000004");

        // Options for Data Collection
        private static readonly Guid EtOptDcInterviewId = Guid.Parse("ee000000-0008-0000-0000-000000000001");
        private static readonly Guid EtOptDcSurveyId = Guid.Parse("ee000000-0008-0000-0000-000000000002");
        private static readonly Guid EtOptDcObservationId = Guid.Parse("ee000000-0008-0000-0000-000000000003");
        private static readonly Guid EtOptDcRepoMiningId = Guid.Parse("ee000000-0008-0000-0000-000000000004");
        private static readonly Guid EtOptDcExperimentId = Guid.Parse("ee000000-0008-0000-0000-000000000005");
        private static readonly Guid EtOptDcDocumentId = Guid.Parse("ee000000-0008-0000-0000-000000000006");
        private static readonly Guid EtOptDcMixedId = Guid.Parse("ee000000-0008-0000-0000-000000000007");

        // ── Section 4: Interventions & Approaches Fields ──────────────────
        private static readonly Guid EtF4InterventionNameId = Guid.Parse("ee000000-0002-0003-0000-000000000001");
        private static readonly Guid EtF4InterventionTypeId = Guid.Parse("ee000000-0002-0003-0000-000000000002");
        private static readonly Guid EtF4ComparisonId = Guid.Parse("ee000000-0002-0003-0000-000000000003");
        private static readonly Guid EtF4TechnologyId = Guid.Parse("ee000000-0002-0003-0000-000000000004");
        private static readonly Guid EtF4AutomationId = Guid.Parse("ee000000-0002-0003-0000-000000000005");
        private static readonly Guid EtF4ToolsId = Guid.Parse("ee000000-0002-0003-0000-000000000006");
        private static readonly Guid EtF4ImplementationId = Guid.Parse("ee000000-0002-0003-0000-000000000007");

        // Options for Intervention Type
        private static readonly Guid EtOptItMethodId = Guid.Parse("ee000000-0009-0000-0000-000000000001");
        private static readonly Guid EtOptItToolId = Guid.Parse("ee000000-0009-0000-0000-000000000002");
        private static readonly Guid EtOptItFrameworkId = Guid.Parse("ee000000-0009-0000-0000-000000000003");
        private static readonly Guid EtOptItProcessId = Guid.Parse("ee000000-0009-0000-0000-000000000004");
        private static readonly Guid EtOptItModelId = Guid.Parse("ee000000-0009-0000-0000-000000000005");
        private static readonly Guid EtOptItOtherId = Guid.Parse("ee000000-0009-0000-0000-000000000006");

        // Options for Automation Level
        private static readonly Guid EtOptAutoFullId = Guid.Parse("ee000000-0010-0000-0000-000000000001");
        private static readonly Guid EtOptAutoSemiId = Guid.Parse("ee000000-0010-0000-0000-000000000002");
        private static readonly Guid EtOptAutoManualId = Guid.Parse("ee000000-0010-0000-0000-000000000003");
        private static readonly Guid EtOptAutoNaId = Guid.Parse("ee000000-0010-0000-0000-000000000004");

        // ── Section 5: Metrics & Outcomes Matrix Fields ───────────────────
        private static readonly Guid EtF5MetricNameId = Guid.Parse("ee000000-0002-0004-0000-000000000001");
        private static readonly Guid EtF5MetricTypeId = Guid.Parse("ee000000-0002-0004-0000-000000000002");
        private static readonly Guid EtF5ValueMeasureId = Guid.Parse("ee000000-0002-0004-0000-000000000003");
        private static readonly Guid EtF5ImprovementId = Guid.Parse("ee000000-0002-0004-0000-000000000004");
        private static readonly Guid EtF5StatSignId = Guid.Parse("ee000000-0002-0004-0000-000000000005");
        private static readonly Guid EtF5NoteId = Guid.Parse("ee000000-0002-0004-0000-000000000006");

        // Options for Metric Category
        private static readonly Guid EtOptMtEffectivenessId = Guid.Parse("ee000000-0011-0000-0000-000000000001");
        private static readonly Guid EtOptMtEfficiencyId = Guid.Parse("ee000000-0011-0000-0000-000000000002");
        private static readonly Guid EtOptMtQualityId = Guid.Parse("ee000000-0011-0000-0000-000000000003");
        private static readonly Guid EtOptMtSatisfactionId = Guid.Parse("ee000000-0011-0000-0000-000000000004");
        private static readonly Guid EtOptMtProductivityId = Guid.Parse("ee000000-0011-0000-0000-000000000005");
        private static readonly Guid EtOptMtCostId = Guid.Parse("ee000000-0011-0000-0000-000000000006");
        private static readonly Guid EtOptMtOtherId = Guid.Parse("ee000000-0011-0000-0000-000000000007");

        // Matrix Columns for Section 5
        private static readonly Guid EtMatCol1ProposedId = Guid.Parse("ee000000-0012-0000-0000-000000000001");
        private static readonly Guid EtMatCol2BaselineId = Guid.Parse("ee000000-0012-0000-0000-000000000002");

        private static readonly Guid SynthesisStrategy1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");


        private static readonly Guid Har2SearchSource1Id = Guid.Parse("b1111111-2222-3333-4444-555555555555");
        private static readonly Guid Har2SearchSource2Id = Guid.Parse("b2222222-2222-3333-4444-555555555555");
        private static readonly Guid Har2SelectionCriteria1Id = Guid.Parse("c1111111-2222-3333-4444-555555555555");
        private static readonly Guid Har2InclusionCriterion1Id = Guid.Parse("c2222222-2222-3333-4444-555555555555");
        private static readonly Guid Har2ExclusionCriterion1Id = Guid.Parse("c3333333-2222-3333-4444-555555555555");

        private static readonly Guid Har2QualityStrategy1Id = Guid.Parse("e1111111-2222-3333-4444-555555555555");
        private static readonly Guid Har2QualityChecklist1Id = Guid.Parse("e2222222-2222-3333-4444-555555555555");
        private static readonly Guid Har2QualityCriterion1Id = Guid.Parse("e3333333-2222-3333-4444-555555555555");
        private static readonly Guid Har2QualityCriterion2Id = Guid.Parse("e4444444-2222-3333-4444-555555555555");
        private static readonly Guid Har2SynthesisStrategy1Id = Guid.Parse("f1111111-2222-3333-4444-555555555555");

        // ── Checklist Template IDs ─────────────────────────────────────
        private static readonly Guid PrismaMainChecklistTemplateId = Guid.Parse("90000000-0000-0000-0000-000000000001");
        private static readonly Guid PrismaAbstractChecklistTemplateId = Guid.Parse("90000000-0000-0000-0000-000000000002");


        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            await SeedUsersAsync(context, passwordHasher);
            await SeedProjectsAsync(context);

            // ── Protocol Planning Phase ─────────────────────────────
            await SeedCoreGovernanceAsync(context);
            await SeedReviewProcessesAsync(context);

            // Seed Processes first to satisfy foreign key dependencies
            await SeedIdentificationProcessesAsync(context);
            await SeedStudySelectionProcessesAsync(context);
            await SeedQualityAssessmentProcessesAsync(context);
            await SeedDataExtractionProcessesAsync(context);
            await SeedSynthesisProcessesAsync(context);

            // Then seed details that depend on those processes
            await SeedSearchSourcesAsync(context);
            await SeedStudySelectionCriteriaAsync(context);
            await SeedQualityAssessmentAsync(context);
            await SeedDataExtractionTemplateAsync(context);
            await SeedDataSynthesisAsync(context);

            await SeedImportBatchesAsync(context);
            await SeedPapersAsync(context);
            await SeedScreeningResolutionsAsync(context);
            await SeedChecklistTemplatesAsync(context);

            await SeedAuditLogsAsync(context);
        }

        private static async Task SeedUsersAsync(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            if (await context.Users.AnyAsync(x => x.Id == AdminUserId || x.Id == ClientUserId))
            {
                return;
            }

            var users = new List<User>
            {
                new User
                {
                    Id = AdminUserId,
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = "admin@srss.com",
                    Role = Role.Admin,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = ClientUserId,
                    Username = "client",
                    FullName = "Demo Client",
                    Email = "client@srss.com",
                    Role = Role.Client,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "nguyenvana",
                    FullName = "Nguyễn Văn A",
                    Email = "nguyen.van.a@srss.com",
                    Role = Role.Client,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                    ModifiedAt = DateTimeOffset.UtcNow.AddDays(-10)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "tran_thib",
                    FullName = "Trần Thị B",
                    Email = "tranthib.demo@srss.com",
                    Role = Role.Client,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-8),
                    ModifiedAt = DateTimeOffset.UtcNow.AddDays(-7)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "hoang_le_c",
                    FullName = "Lê Hoàng C",
                    Email = "lehoangc@srss.com",
                    Role = Role.Client,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-5),
                    ModifiedAt = DateTimeOffset.UtcNow.AddDays(-5)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "phamminhd",
                    FullName = "Phạm Minh D",
                    Email = "minh.pham@srss.com",
                    Role = Role.Client,
                    IsActive = false, // Giả định một user đang bị khóa để test logic UI
					CreatedAt = DateTimeOffset.UtcNow.AddDays(-3),
                    ModifiedAt = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "vuthie",
                    FullName = "Vũ Thị E",
                    Email = "vuthie.client@srss.com",
                    Role = Role.Client,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }

            };

            foreach (var user in users)
            {
                user.Password = passwordHasher.HashPassword(user, "123456");
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProjectsAsync(AppDbContext context)
        {
            if (await context.SystematicReviewProjects.AnyAsync(x => x.Id == HarProjectId || x.Id == ScientificLiteracyProjectId))
            {
                return;
            }

            var projects = new List<SystematicReviewProject> {
                new SystematicReviewProject{
                    Id = HarProjectId,
                    Title = "AI-based Human Activity Recognition",
                    Domain = "Computer Science",
                    Description = "Systematic review of AI-based HAR approaches using deep learning and machine learning techniques",
                    Status = ProjectStatus.Active,
                    StartDate = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new SystematicReviewProject{
                    Id = ScientificLiteracyProjectId,
                    Title = "Empowering Scientific Literacy of Science Teachers: A Systematic Literature Review",
                    Domain = "Science Education",
                    Description = "A systematic literature review investigating definitions, strategies, and research trends related to empowering scientific literacy among science teachers.",
                    Status = ProjectStatus.Completed,
                    StartDate = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },

            };

            await context.SystematicReviewProjects.AddRangeAsync(projects);
            await context.SaveChangesAsync();
        }

        private static async Task SeedReviewProcessesAsync(AppDbContext context)
        {
            if (await context.ReviewProcesses.AnyAsync(x => x.Id == HarReviewProcessId))
            {
                return;
            }

            var reviewProcess = new ReviewProcess
            {
                Id = HarReviewProcessId,
                Name = "HAR Systematic Review Process",
                ProjectId = HarProjectId,
                CurrentPhase = ProcessPhase.Identification,
                Status = ProcessStatus.InProgress,
                StartedAt = DateTimeOffset.UtcNow,
                Notes = "Initial review process for HAR systematic review",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.ReviewProcesses.AddAsync(reviewProcess);
            await context.SaveChangesAsync();
        }

        private static async Task SeedIdentificationProcessesAsync(AppDbContext context)
        {
            if (await context.IdentificationProcesses.AnyAsync(x => x.Id == HarIdentificationProcessId))
            {
                return;
            }

            var identificationProcess = new IdentificationProcess
            {
                Id = HarIdentificationProcessId,
                ReviewProcessId = HarReviewProcessId,
                Status = IdentificationStatus.InProgress,
                StartedAt = DateTimeOffset.UtcNow,
                Notes = "Identification phase: searching for HAR papers in multiple databases",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.IdentificationProcesses.AddAsync(identificationProcess);
            await context.SaveChangesAsync();
        }

        private static async Task SeedStudySelectionProcessesAsync(AppDbContext context)
        {
            if (await context.StudySelectionProcesses.AnyAsync(x => x.Id == HarStudySelectionProcessId))
            {
                return;
            }
            var studySelectionProcess = new StudySelectionProcess
            {
                Id = HarStudySelectionProcessId,
                ReviewProcessId = HarReviewProcessId,
                Status = SelectionProcessStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
            await context.StudySelectionProcesses.AddAsync(studySelectionProcess);
            await context.SaveChangesAsync();
        }

        private static async Task SeedQualityAssessmentProcessesAsync(AppDbContext context)
        {
            if (await context.QualityAssessmentProcesses.AnyAsync(x => x.Id == HarQualityAssessmentProcessId))
            {
                return;
            }
            var qualityAssessmentProcess = new QualityAssessmentProcess
            {
                Id = HarQualityAssessmentProcessId,
                ReviewProcessId = HarReviewProcessId,
                Status = QualityAssessmentProcessStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
            await context.QualityAssessmentProcesses.AddAsync(qualityAssessmentProcess);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDataExtractionProcessesAsync(AppDbContext context)
        {
            if (await context.DataExtractionProcesses.AnyAsync(x => x.Id == HarDataExtractionProcessId))
            {
                return;
            }
            var dataExtractionProcess = new DataExtractionProcess
            {
                Id = HarDataExtractionProcessId,
                ReviewProcessId = HarReviewProcessId,
                Status = ExtractionProcessStatus.NotStarted,
                Notes = "Auto-created data extraction process mock",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
            await context.DataExtractionProcesses.AddAsync(dataExtractionProcess);
            await context.SaveChangesAsync();
        }

        private static async Task SeedSynthesisProcessesAsync(AppDbContext context)
        {
            if (await context.SynthesisProcesses.AnyAsync(x => x.Id == HarSynthesisProcessId))
            {
                return;
            }
            var synthesisProcess = new SynthesisProcess
            {
                Id = HarSynthesisProcessId,
                ReviewProcessId = HarReviewProcessId,
                Status = SynthesisProcessStatus.NotStarted,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };
            await context.SynthesisProcesses.AddAsync(synthesisProcess);
            await context.SaveChangesAsync();
        }



        private static async Task SeedImportBatchesAsync(AppDbContext context)
        {
            if (await context.ImportBatches.AnyAsync(x => x.Id == ScopusImportBatchId))
            {
                return;
            }

            var importBatch = new ImportBatch
            {
                Id = ScopusImportBatchId,
                ProjectId = HarProjectId,
                FileName = "scopus_har_results.ris",
                FileType = "RIS",
                Source = "Scopus",
                ImportedBy = "demo_user",
                ImportedAt = DateTimeOffset.UtcNow,
                TotalRecords = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.ImportBatches.AddAsync(importBatch);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPapersAsync(AppDbContext context)
        {
            if (await context.Papers.AnyAsync(x => x.Id == Paper1Id || x.Id == Paper2Id || x.Id == Paper3Id))
            {
                return;
            }

            var papers = new List<Paper>
            {
                new Paper
                {
                    Id = Paper1Id,
                    Title = "Deep Learning for Human Activity Recognition: A Survey",
                    Authors = "Wang, J., Chen, Y., Hao, S., Peng, X., Hu, L.",
                    Abstract = "Human activity recognition (HAR) plays a crucial role in various applications including healthcare monitoring, smart homes, and assisted living. This comprehensive survey examines state-of-the-art deep learning approaches for HAR, including convolutional neural networks (CNN), long short-term memory networks (LSTM), and hybrid architectures. We analyze their performance across different sensor modalities and activity types.",
                    DOI = "10.1109/TPAMI.2021.3050234",
                    PublicationType = "Journal Article",
                    PublicationYear = "2021",
                    PublicationYearInt = 2021,
                    Journal = "IEEE Transactions on Pattern Analysis and Machine Intelligence",
                    Volume = "43",
                    Issue = "11",
                    Pages = "3852-3874",
                    Keywords = "Human Activity Recognition; Deep Learning; CNN; LSTM; Sensor Data; Survey",
                    Url = "https://doi.org/10.1109/TPAMI.2021.3050234",
                    ProjectId = HarProjectId,
                    ImportBatchId = ScopusImportBatchId,
                    Source = "RIS",
                    ImportedAt = DateTimeOffset.UtcNow,
                    ImportedBy = "demo_user",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new Paper
                {
                    Id = Paper2Id,
                    Title = "Attention-Based Convolutional Neural Networks for Human Activity Recognition Using Wearable Sensors",
                    Authors = "Xia, K., Huang, J., Wang, H.",
                    Abstract = "This paper proposes an attention-based convolutional neural network (CNN) architecture for human activity recognition using wearable sensor data. The attention mechanism enables the model to automatically focus on discriminative temporal features, improving recognition accuracy. Experimental results on benchmark datasets demonstrate the effectiveness of the proposed approach.",
                    DOI = "10.1016/j.patcog.2020.107458",
                    PublicationType = "Journal Article",
                    PublicationYear = "2020",
                    PublicationYearInt = 2020,
                    Journal = "Pattern Recognition",
                    Volume = "105",
                    Pages = "107458",
                    Keywords = "Human Activity Recognition; Attention Mechanism; CNN; Wearable Sensors; Time Series",
                    Url = "https://doi.org/10.1016/j.patcog.2020.107458",
                    ProjectId = HarProjectId,
                    ImportBatchId = ScopusImportBatchId,
                    Source = "RIS",
                    ImportedAt = DateTimeOffset.UtcNow,
                    ImportedBy = "demo_user",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new Paper
                {
                    Id = Paper3Id,
                    Title = "Ensemble Deep Learning for Skeleton-Based Action Recognition Using Temporal Sliding LSTM Networks",
                    Authors = "Lee, I., Kim, D., Kang, S., Lee, S.",
                    Abstract = "We propose an ensemble deep learning framework using temporal sliding LSTM networks for skeleton-based action recognition. The temporal sliding window approach captures multi-scale temporal dynamics, while the ensemble strategy enhances robustness. Comprehensive experiments on NTU RGB+D and Kinetics datasets demonstrate superior performance compared to state-of-the-art methods.",
                    DOI = "10.1109/ICCV.2019.00312",
                    PublicationType = "Conference Paper",
                    PublicationYear = "2019",
                    PublicationYearInt = 2019,
                    Journal = "Proceedings of the IEEE International Conference on Computer Vision",
                    ConferenceName = "IEEE International Conference on Computer Vision",
                    ConferenceLocation = "Seoul, South Korea",
                    ConferenceStartDate = new DateTimeOffset(2019, 10, 27, 0, 0, 0, TimeSpan.Zero),
                    ConferenceEndDate = new DateTimeOffset(2019, 11, 2, 0, 0, 0, TimeSpan.Zero),
                    ConferenceYear = 2019,
                    Pages = "3024-3033",
                    Keywords = "Action Recognition; LSTM; Skeleton Data; Ensemble Learning; Deep Learning; Temporal Modeling",
                    Url = "https://doi.org/10.1109/ICCV.2019.00312",
                    ProjectId = HarProjectId,
                    ImportBatchId = ScopusImportBatchId,
                    Source = "RIS",
                    ImportedAt = DateTimeOffset.UtcNow,
                    ImportedBy = "demo_user",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            await context.Papers.AddRangeAsync(papers);
            await context.SaveChangesAsync();
        }

        private static async Task SeedScreeningResolutionsAsync(AppDbContext context)
        {
            if (await context.ScreeningResolutions.AnyAsync(x => x.Id == ScreeningRes1Id || x.Id == ScreeningRes2Id || x.Id == ScreeningRes3Id))
            {
                return;
            }

            // Mock that Paper1 and Paper2 PASSED the Full-Text Screening.
            // Mock that Paper3 FAILED the Full-Text Screening.
            var resolutions = new List<ScreeningResolution>
            {
                new ScreeningResolution
                {
                    Id = ScreeningRes1Id,
                    StudySelectionProcessId = HarStudySelectionProcessId,
                    PaperId = Paper1Id,
                    Phase = ScreeningPhase.FullText,
                    FinalDecision = ScreeningDecisionType.Include,
                    ResolvedAt = DateTimeOffset.UtcNow,
                    ResolvedBy = AdminUserId,
                    ResolutionNotes = "Paper clearly matching all inclusion criteria",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ScreeningResolution
                {
                    Id = ScreeningRes2Id,
                    StudySelectionProcessId = HarStudySelectionProcessId,
                    PaperId = Paper2Id,
                    Phase = ScreeningPhase.FullText,
                    FinalDecision = ScreeningDecisionType.Include,
                    ResolvedAt = DateTimeOffset.UtcNow,
                    ResolvedBy = AdminUserId,
                    ResolutionNotes = "Excellent empirical setup, full text looks great",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ScreeningResolution
                {
                    Id = ScreeningRes3Id,
                    StudySelectionProcessId = HarStudySelectionProcessId,
                    PaperId = Paper3Id,
                    Phase = ScreeningPhase.FullText,
                    FinalDecision = ScreeningDecisionType.Exclude,
                    ResolvedAt = DateTimeOffset.UtcNow,
                    ResolvedBy = ClientUserId,
                    ResolutionNotes = "Failed on exclusion criteria #2",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            await context.ScreeningResolutions.AddRangeAsync(resolutions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedCoreGovernanceAsync(AppDbContext context)
        {
            // ── ReviewNeed ──────────────────────────────────────────────────
            if (!await context.ReviewNeeds.AnyAsync(x => x.Id == ReviewNeed1Id || x.Id == ReviewNeed2Id))
            {
                var reviewNeeds = new List<ReviewNeed>
                {
                    new ReviewNeed
                    {
                        Id = ReviewNeed1Id,
                        ProjectId = HarProjectId,
                        Description = "Systematic review needed to evaluate deep learning approaches for human activity recognition using wearable sensors",
                        Justification = "Current literature lacks a comprehensive comparison of DL architectures under real-world conditions with limited labelled data",
                        IdentifiedBy = "Dr. Nguyen",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ReviewNeed
                    {
                        Id = ReviewNeed2Id,
                        ProjectId = HarProjectId,
                        Description = "Evidence synthesis required on transfer learning effectiveness for cross-subject HAR generalisation",
                        Justification = "Inconsistent findings across studies motivate a pooled analysis to guide future model selection",
                        IdentifiedBy = "Research Team",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.ReviewNeeds.AddRangeAsync(reviewNeeds);
                await context.SaveChangesAsync();
            }

            // ── CommissioningDocument ────────────────────────────────────────
            if (!await context.CommissioningDocuments.AnyAsync(x => x.Id == CommissioningDocumentId))
            {
                var commissioningDocument = new CommissioningDocument
                {
                    Id = CommissioningDocumentId,
                    ProjectId = HarProjectId,
                    Sponsor = "Vietnam National University – Ho Chi Minh City",
                    Scope = "Review covers deep learning and traditional ML methods applied to HAR using inertial measurement unit (IMU) and camera-based sensors, published 2015–2025",
                    Budget = 15000.00m,
                    DocumentUrl = "https://internal.vnuhcm.edu.vn/docs/slr-har-commission-2025.pdf",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                };

                await context.CommissioningDocuments.AddAsync(commissioningDocument);
                await context.SaveChangesAsync();
            }

            // ── ReviewObjective ──────────────────────────────────────────────
            if (!await context.ReviewObjectives.AnyAsync(x => x.Id == ReviewObjective1Id || x.Id == ReviewObjective2Id))
            {
                var reviewObjectives = new List<ReviewObjective>
                {
                    new ReviewObjective
                    {
                        Id = ReviewObjective1Id,
                        ProjectId = HarProjectId,
                        ObjectiveStatement = "To identify and compare deep learning architectures used for human activity recognition with wearable sensor data, assessing their accuracy, computational cost, and generalisation capability",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ReviewObjective
                    {
                        Id = ReviewObjective2Id,
                        ProjectId = HarProjectId,
                        ObjectiveStatement = "To evaluate the impact of data augmentation and transfer learning strategies on HAR model performance across different subject populations",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.ReviewObjectives.AddRangeAsync(reviewObjectives);
                await context.SaveChangesAsync();
            }

            // ── QuestionType ─────────────────────────────────────────────────
            if (!await context.QuestionTypes.AnyAsync(x => x.Id == QuestionTypePicocId || x.Id == QuestionTypePicoId || x.Id == QuestionTypeGqmId))
            {
                var questionTypes = new List<QuestionType>
                {
                    new QuestionType
                    {
                        Id = QuestionTypePicocId,
                        Name = "PICOC",
                        Description = "Population, Intervention, Comparison, Outcome, Context – standard SLR question framework",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new QuestionType
                    {
                        Id = QuestionTypePicoId,
                        Name = "PICO",
                        Description = "Population, Intervention, Comparison, Outcome – clinical research question framework without explicit context",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new QuestionType
                    {
                        Id = QuestionTypeGqmId,
                        Name = "GQM",
                        Description = "Goal, Question, Metric – software engineering measurement framework",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.QuestionTypes.AddRangeAsync(questionTypes);
                await context.SaveChangesAsync();
            }

            // ── ResearchQuestion + PICOC ─────────────────────────────────────
            if (!await context.ResearchQuestions.AnyAsync(x => x.Id == ResearchQuestion1Id || x.Id == ResearchQuestion2Id))
            {
                var researchQuestions = new List<ResearchQuestion>
                {
                    new ResearchQuestion
                    {
                        Id = ResearchQuestion1Id,
                        ProjectId = HarProjectId,
                        QuestionTypeId = QuestionTypePicocId,
                        QuestionText = "What deep learning approaches are most effective for human activity recognition using wearable sensors in terms of accuracy and computational efficiency?",
                        Rationale = "Understanding the performance trade-offs of different DL architectures will guide practitioners in selecting appropriate models for resource-constrained devices",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ResearchQuestion
                    {
                        Id = ResearchQuestion2Id,
                        ProjectId = HarProjectId,
                        QuestionTypeId = QuestionTypePicocId,
                        QuestionText = "How does transfer learning affect the generalisation of HAR models across different subjects and activity sets?",
                        Rationale = "Cross-subject generalisation is a known bottleneck; synthesising transfer learning evidence will inform future benchmark design",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.ResearchQuestions.AddRangeAsync(researchQuestions);
                await context.SaveChangesAsync();

                // ── PicocElements for RQ1 ────────────────────────────────────
                var picocElementsRq1 = new List<PicocElement>
                {
                    new PicocElement { Id = Rq1PopulationId, ResearchQuestionId = ResearchQuestion1Id, ElementType = "Population",     Description = "Adults performing daily living activities monitored by wearable IMU sensors", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq1InterventionId, ResearchQuestionId = ResearchQuestion1Id, ElementType = "Intervention",   Description = "Deep learning models (CNN, LSTM, Transformer) applied to raw sensor time-series data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq1ComparisonId, ResearchQuestionId = ResearchQuestion1Id, ElementType = "Comparison",     Description = "Traditional machine learning classifiers (SVM, Random Forest, k-NN)", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq1OutcomeId, ResearchQuestionId = ResearchQuestion1Id, ElementType = "Outcome",        Description = "Recognition accuracy (F1-score), inference latency, and model size on benchmark datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq1ContextId, ResearchQuestionId = ResearchQuestion1Id, ElementType = "Context",        Description = "Controlled laboratory and free-living environments using publicly available HAR datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                };

                await context.PicocElements.AddRangeAsync(picocElementsRq1);
                await context.SaveChangesAsync();

                // child detail rows for RQ1 PICOC elements
                await context.Populations.AddAsync(new Population { Id = Rq1PopulationDetailId, PicocId = Rq1PopulationId, Description = "Healthy adults (18–65 years) in both controlled-lab and free-living settings", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Rq1InterventionDetailId, PicocId = Rq1InterventionId, Description = "End-to-end deep learning pipelines trained on windowed accelerometer/gyroscope data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison { Id = Rq1ComparisonDetailId, PicocId = Rq1ComparisonId, Description = "Hand-crafted feature extraction followed by classical ML classifiers", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome { Id = Rq1OutcomeDetailId, PicocId = Rq1OutcomeId, Metric = "F1-score, Accuracy, Inference Time (ms)", Description = "Per-activity and macro-average F1-score on UCI HAR, OPPORTUNITY, and PAMAP2 datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context { Id = Rq1ContextDetailId, PicocId = Rq1ContextId, Environment = "Laboratory and free-living", Description = "Studies published in English in peer-reviewed venues between 2015 and 2025", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();

                // ── PicocElements for RQ2 ────────────────────────────────────
                var picocElementsRq2 = new List<PicocElement>
                {
                    new PicocElement { Id = Rq2PopulationId, ResearchQuestionId = ResearchQuestion2Id, ElementType = "Population",     Description = "Heterogeneous subject groups across age, fitness level, and activity repertoire", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq2InterventionId, ResearchQuestionId = ResearchQuestion2Id, ElementType = "Intervention",   Description = "Pre-trained deep learning models fine-tuned on target subject data (transfer learning)", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq2ComparisonId, ResearchQuestionId = ResearchQuestion2Id, ElementType = "Comparison",     Description = "Models trained from scratch on target subject data without pre-training", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq2OutcomeId, ResearchQuestionId = ResearchQuestion2Id, ElementType = "Outcome",        Description = "Cross-subject accuracy improvement and training data reduction achieved by transfer learning", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = Rq2ContextId, ResearchQuestionId = ResearchQuestion2Id, ElementType = "Context",        Description = "Subject-independent and leave-one-subject-out evaluation protocols", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                };

                await context.PicocElements.AddRangeAsync(picocElementsRq2);
                await context.SaveChangesAsync();

                // child detail rows for RQ2 PICOC elements
                await context.Populations.AddAsync(new Population { Id = Rq2PopulationDetailId, PicocId = Rq2PopulationId, Description = "Subjects with varying demographics in leave-one-subject-out cross-validation splits", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Rq2InterventionDetailId, PicocId = Rq2InterventionId, Description = "Domain adaptation and fine-tuning strategies using source-domain pre-trained weights", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison { Id = Rq2ComparisonDetailId, PicocId = Rq2ComparisonId, Description = "Baseline models trained exclusively on target subject data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome { Id = Rq2OutcomeDetailId, PicocId = Rq2OutcomeId, Metric = "Accuracy gain (%), Required labelled samples", Description = "Reduction in labelled training samples required to match scratch-trained model accuracy", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context { Id = Rq2ContextDetailId, PicocId = Rq2ContextId, Environment = "Cross-subject evaluation", Description = "HAR datasets supporting subject-independent splits: NTU RGB+D, PAMAP2, OPPORTUNITY", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSearchSourcesAsync(AppDbContext context)
        {
            if (!await context.SearchSources.AnyAsync(x => x.Id == Har2SearchSource1Id))
            {
                await context.SearchSources.AddRangeAsync(new List<SearchSource>
                {
                    new SearchSource { Id = Har2SearchSource1Id, ProjectId = HarProjectId, Name = "Scopus", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new SearchSource { Id = Har2SearchSource2Id, ProjectId = HarProjectId, Name = "Web of Science", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedStudySelectionCriteriaAsync(AppDbContext context)
        {
            if (!await context.StudySelectionCriterias.AnyAsync(x => x.Id == Har2SelectionCriteria1Id))
            {
                await context.StudySelectionCriterias.AddAsync(new StudySelectionCriteria { Id = Har2SelectionCriteria1Id, StudySelectionProcessId = HarStudySelectionProcessId, Description = "Criteria for HAR protocol v2", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.InclusionCriteria.AddAsync(new InclusionCriterion { Id = Har2InclusionCriterion1Id, CriteriaId = Har2SelectionCriteria1Id, Rule = "Deep Learning applied to IMU data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.ExclusionCriteria.AddAsync(new ExclusionCriterion { Id = Har2ExclusionCriterion1Id, CriteriaId = Har2SelectionCriteria1Id, Rule = "Not published in English", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedQualityAssessmentAsync(AppDbContext context)
        {
            if (!await context.QualityAssessmentStrategies.AnyAsync(x => x.Id == Har2QualityStrategy1Id))
            {
                await context.QualityAssessmentStrategies.AddAsync(new QualityAssessmentStrategy { Id = Har2QualityStrategy1Id, ReviewProcessId = HarReviewProcessId, Description = "Custom checklist for Protocol 2", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.QualityChecklists.AddAsync(new QualityChecklist { Id = Har2QualityChecklist1Id, QaStrategyId = Har2QualityStrategy1Id, Name = "HAR Protocol 2 Checklist", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.QualityCriteria.AddRangeAsync(new List<QualityCriterion>
                {
                    new QualityCriterion { Id = Har2QualityCriterion1Id, ChecklistId = Har2QualityChecklist1Id, Question = "Is the paper based on research (or is it a discussion paper based on expert opinion)?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Har2QualityCriterion2Id, ChecklistId = Har2QualityChecklist1Id, Question = "What research method was used: Experiment, Quasi-Experiment, Lessons learnt, Case study, Opinion Survey, Tertiary Study, Other (specify)? Note This is to be based on our reading of the paper not the method claimed by the author of the paper.", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Is there a clear statement of the aims of the study?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Is there an adequate description of the context in which the research or observation was carried out?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Was the research method appropriate to address the aims of the research? (i.e. Expert Opinion).", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Was the recruitment strategy (for human-based experiments and quasi-experiments) or experimental material or context (for lessons learnt) appropriate to the aims of the research?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "For empirical studies (apart from Lessons Learnt), was there a control group or baseline with which to evaluate SR procedures/techniques?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "For empirical studies (apart from Lessons Learnt), was the data collected in a way that addressed the research issue?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "For empirical studies (apart from Lessons Learnt), was the data analysis sufficiently rigorous?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Has the relationship between researcher and participants been considered to an adequate degree?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Is there a clear statement of findings?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new QualityCriterion { Id = Guid.NewGuid(), ChecklistId = Har2QualityChecklist1Id, Question = "Is the study of value for research or practice?", Weight = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDataSynthesisAsync(AppDbContext context)
        {
            if (!await context.DataSynthesisStrategies.AnyAsync(x => x.Id == Har2SynthesisStrategy1Id))
            {
                await context.DataSynthesisStrategies.AddAsync(new DataSynthesisStrategy { Id = Har2SynthesisStrategy1Id, SynthesisProcessId = HarSynthesisProcessId, SynthesisType = SynthesisType.NarrativeThematic, DataGroupingPlan = "Studies will be stratified structurally by SE methodology (e.g., Agile, DevOps) and further categorized by company size (SME vs Enterprise).", SensitivityAnalysisPlan = "We will perform a sensitivity analysis by temporarily excluding studies flagged as 'Low Quality' during the formal Quality Assessment phase to observe if they inflate error ranges or skew the general narrative findings.", Description = "Narrative synthesis of contextual factors and outcomes extracted from primary studies based on thematic clustering.", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDataExtractionTemplateAsync(AppDbContext context)
        {
            if (await context.ExtractionTemplates.AnyAsync(x => x.Id == HarExtractionTemplateId))
            {
                return;
            }

            // ── Template ────────────────────────────────────────────────────
            var template = new ExtractionTemplate
            {
                Id = HarExtractionTemplateId,
                DataExtractionProcessId = HarDataExtractionProcessId,
                Name = "AI-HAR Systematic Review Template",
                Description = "A methodology-compliant template for extracting data from AI-based Human Activity Recognition studies, structured around PICOC and Research Questions.",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.ExtractionTemplates.AddAsync(template);
            await context.SaveChangesAsync();

            // ── Sections ─────────────────────────────────────────────────────
            var sections = new List<ExtractionSection>
            {
                // Section 1: PICOC & Study Context
                new ExtractionSection
                {
                    Id = EtSec1IdentificationId,
                    TemplateId = HarExtractionTemplateId,
                    Name = "PICOC & Study Context",
                    Description = "Standard PICOC metadata and study context information.",
                    SectionType = SectionType.FlatForm,
                    IsPicoc = true,
                    OrderIndex = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                // Section 2: RQ-Driven Data
                new ExtractionSection
                {
                    Id = EtSec2ContextId,
                    TemplateId = HarExtractionTemplateId,
                    Name = "RQ1: What deep learning approaches are most effective for human activity recognition using wearable sensors in terms of accuracy and computational efficiency?",
                    Description = "Data extracted specifically to answer Research Question 1 regarding AI approaches and efficiency.",
                    SectionType = SectionType.FlatForm,
                    LinkedResearchQuestionId = ResearchQuestion1Id,
                    IsPicoc = false,
                    OrderIndex = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionSection
                {
                    Id = EtSec3ContextId,
                    TemplateId = HarExtractionTemplateId,
                    Name = "RQ2: How do different deep learning architectures compare in terms of accuracy, computational efficiency, and generalization performance across diverse datasets and scenarios?",
                    Description = "Data extracted specifically to answer Research Question 2 regarding deep learning architectures.",
                    SectionType = SectionType.FlatForm,
                    LinkedResearchQuestionId = ResearchQuestion2Id,
                    IsPicoc = false,
                    OrderIndex = 2,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            await context.ExtractionSections.AddRangeAsync(sections);
            await context.SaveChangesAsync();

            // ── Section 1 Fields: PICOC ─────────────────────────────────────
            var sec1Fields = new List<ExtractionField>
            {
                new ExtractionField
                {
                    Id = EtF1StudyIdId,
                    SectionId = EtSec1IdentificationId,
                    Name = "Population",
                    Instruction = "Describe the population studied.",
                    FieldType = FieldType.Text,
                    IsRequired = false,
                    OrderIndex = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF1ContextId,
                    SectionId = EtSec1IdentificationId,
                    Name = "Context",
                    Instruction = "Describe the context of the study.",
                    FieldType = FieldType.Text,
                    IsRequired = false,
                    OrderIndex = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF1InterventionId,
                    SectionId = EtSec1IdentificationId,
                    Name = "Intervention",
                    Instruction = "Describe the intervention of the study.",
                    FieldType = FieldType.Text,
                    IsRequired = false,
                    OrderIndex = 2,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF1ComparisonId,
                    SectionId = EtSec1IdentificationId,
                    Name = "Comparison",
                    Instruction = "Describe the comparison of the study.",
                    FieldType = FieldType.Text,
                    IsRequired = false,
                    OrderIndex = 3,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF1OutcomeId,
                    SectionId = EtSec1IdentificationId,
                    Name = "Outcome",
                    Instruction = "Describe the outcome of the study.",
                    FieldType = FieldType.Text,
                    IsRequired = false,
                    OrderIndex = 4,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            await context.ExtractionFields.AddRangeAsync(sec1Fields);

            // ── Section 1 Options ───────────────────────────────────────────
            var methodologyOptions = new List<FieldOption>
            {
                new FieldOption { Id = EtOptPubJournalId,    FieldId = EtF1StudyIdId, Value = "Case Study", DisplayOrder = 0, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                new FieldOption { Id = EtOptPubConferenceId, FieldId = EtF1StudyIdId, Value = "Controlled Experiment", DisplayOrder = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                new FieldOption { Id = EtOptPubWorkshopId,   FieldId = EtF1StudyIdId, Value = "Action Research", DisplayOrder = 2, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
            };

            var contextOptions = new List<FieldOption>
            {
                new FieldOption { Id = EtOptCtxIndustrialId, FieldId = EtF1ContextId, Value = "Industrial/Real-world", DisplayOrder = 0, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                new FieldOption { Id = EtOptCtxAcademicId,   FieldId = EtF1ContextId, Value = "Academic/Laboratory", DisplayOrder = 1, CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
            };

            await context.FieldOptions.AddRangeAsync(methodologyOptions);
            await context.FieldOptions.AddRangeAsync(contextOptions);

            // ── Section 2 Fields: RQ Driven ─────────────────────────────────
            var sec2Fields = new List<ExtractionField>
            {
                new ExtractionField
                {
                    Id = EtF2PopulationId,
                    SectionId = EtSec2ContextId,
                    Name = "AI Tool Name",
                    Instruction = "Enter the name of the AI tool or framework used.",
                    FieldType = FieldType.Text,
                    IsRequired = true,
                    OrderIndex = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF2DomainId,
                    SectionId = EtSec2ContextId,
                    Name = "Performance Improvement %",
                    Instruction = "The reported improvement percentage compared to the baseline.",
                    FieldType = FieldType.Decimal,
                    IsRequired = false,
                    OrderIndex = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            var sec3Fields = new List<ExtractionField>
            {
                new ExtractionField
                {
                    Id = EtF3PopulationId_RQ2,
                    SectionId = EtSec3ContextId,
                    Name = "AI Tool Name",
                    Instruction = "Enter the name of the AI tool or framework used.",
                    FieldType = FieldType.Text,
                    IsRequired = true,
                    OrderIndex = 0,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new ExtractionField
                {
                    Id = EtF3DomainId_RQ2,
                    SectionId = EtSec3ContextId,
                    Name = "Performance Improvement %",
                    Instruction = "The reported improvement percentage compared to the baseline.",
                    FieldType = FieldType.Decimal,
                    IsRequired = false,
                    OrderIndex = 1,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                }
            };

            await context.ExtractionFields.AddRangeAsync(sec2Fields);
            await context.ExtractionFields.AddRangeAsync(sec3Fields);
            await context.SaveChangesAsync();
        }

        private static async Task SeedChecklistTemplatesAsync(AppDbContext context)
        {
            if (await context.ChecklistTemplates.AnyAsync(x => x.Id == PrismaMainChecklistTemplateId || x.Id == PrismaAbstractChecklistTemplateId))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            var mainTemplate = new ChecklistTemplate
            {
                Id = PrismaMainChecklistTemplateId,
                Name = "PRISMA 2020 Main Checklist",
                Description = "Official PRISMA 2020 checklist for full systematic review reports.",
                IsSystem = true,
                Type = ChecklistType.Full,
                Version = now.ToString("yyyyMMddHHmmss"),
                CreatedAt = now,
                UpdatedAt = now
            };

            var abstractTemplate = new ChecklistTemplate
            {
                Id = PrismaAbstractChecklistTemplateId,
                Name = "PRISMA 2020 Abstract Checklist",
                Description = "Official PRISMA 2020 checklist for abstracts of systematic reviews.",
                IsSystem = true,
                Type = ChecklistType.Abstract,
                Version = now.ToString("yyyyMMddHHmmss"),
                CreatedAt = now,
                UpdatedAt = now
            };

            await context.ChecklistTemplates.AddRangeAsync(mainTemplate, abstractTemplate);

            var mainItems = new List<(string Number, string? Parent, string Section, string Topic, string Description, int Order, bool IsRequired)>
            {
                ("1", null, "TITLE", "Title", "Identify the report as a systematic review.", 1, true),
                ("2", null, "ABSTRACT", "Abstract", "See the PRISMA 2020 for Abstracts checklist.", 2, true),
                ("3", null, "INTRODUCTION", "Rationale", "Describe the rationale for the review in the context of existing knowledge.", 3, true),
                ("4", null, "INTRODUCTION", "Objectives", "Provide an explicit statement of the objective(s) or question(s) the review addresses.", 4, true),
                ("5", null, "METHODS", "Eligibility criteria", "Specify the inclusion and exclusion criteria for the review and how studies were grouped for the syntheses.", 5, true),
                ("6", null, "METHODS", "Information sources", "Specify all databases, registers, websites, organisations, reference lists and other sources searched or consulted to identify studies. Specify the date when each source was last searched or consulted.", 6, true),
                ("7", null, "METHODS", "Search strategy", "Present the full search strategies for all databases, registers and websites, including any filters and limits used.", 7, true),
                ("8", null, "METHODS", "Selection process", "Specify the methods used to decide whether a study met the inclusion criteria of the review, including how many reviewers screened each record and each report retrieved, whether they worked independently, and if applicable, details of automation tools used in the process.", 8, true),
                ("9", null, "METHODS", "Data collection process", "Specify the methods used to collect data from reports, including how many reviewers collected data from each report, whether they worked independently, any processes for obtaining or confirming data from study investigators, and if applicable, details of automation tools used in the process.", 9, true),
                ("10", null, "METHODS", "Data items", "Data items grouping node.", 10, true),
                ("10a", "10", "METHODS", "Data items", "List and define all outcomes for which data were sought. Specify whether all results that were compatible with each outcome domain in each study were sought and, if not, the methods used to decide which results to collect.", 11, true),
                ("10b", "10", "METHODS", "Data items", "List and define all other variables for which data were sought (for example, participant and intervention characteristics, funding sources). Describe any assumptions made about any missing or unclear information.", 12, true),
                ("11", null, "METHODS", "Study risk of bias assessment", "Specify the methods used to assess risk of bias in the included studies, including details of the tool(s) used, how many reviewers assessed each study and whether they worked independently, and if applicable, details of automation tools used in the process.", 13, true),
                ("12", null, "METHODS", "Effect measures", "Specify for each outcome the effect measure(s) (for example, risk ratio, mean difference) used in the synthesis or presentation of results.", 14, true),
                ("13", null, "METHODS", "Synthesis methods", "Synthesis methods grouping node.", 15, true),
                ("13a", "13", "METHODS", "Synthesis methods", "Describe the processes used to decide which studies were eligible for each synthesis (for example, tabulating the study intervention characteristics and comparing against the planned groups for each synthesis).", 16, true),
                ("13b", "13", "METHODS", "Synthesis methods", "Describe any methods required to prepare the data for presentation or synthesis, such as handling of missing summary statistics, or data conversions.", 17, true),
                ("13c", "13", "METHODS", "Synthesis methods", "Describe any methods used to tabulate or visually display results of individual studies and syntheses.", 18, true),
                ("13d", "13", "METHODS", "Synthesis methods", "Describe any methods used to synthesize results and provide a rationale for the choice(s). If meta-analysis was performed, describe the model(s), method(s) to identify the presence and extent of statistical heterogeneity, and software package(s) used.", 19, true),
                ("13e", "13", "METHODS", "Synthesis methods", "Describe any methods used to explore possible causes of heterogeneity among study results (for example, subgroup analysis, meta-regression).", 20, true),
                ("13f", "13", "METHODS", "Synthesis methods", "Describe any sensitivity analyses conducted to assess robustness of the synthesized results.", 21, true),
                ("14", null, "METHODS", "Reporting bias assessment", "Describe any methods used to assess risk of bias due to missing results in a synthesis (arising from reporting biases).", 22, true),
                ("15", null, "METHODS", "Certainty assessment", "Describe any methods used to assess certainty (or confidence) in the body of evidence for an outcome.", 23, true),
                ("16", null, "RESULTS", "Study selection", "Study selection grouping node.", 24, true),
                ("16a", "16", "RESULTS", "Study selection", "Describe the results of the search and selection process, from the number of records identified in the search to the number of studies included in the review, ideally using a flow diagram.", 25, true),
                ("16b", "16", "RESULTS", "Study selection", "Cite studies that might appear to meet the inclusion criteria, but which were excluded, and explain why they were excluded.", 26, true),
                ("17", null, "RESULTS", "Study characteristics", "Cite each included study and present its characteristics.", 27, true),
                ("18", null, "RESULTS", "Risk of bias in studies", "Present assessments of risk of bias for each included study.", 28, true),
                ("19", null, "RESULTS", "Results of individual studies", "For all outcomes, present, for each study: (a) summary statistics for each group (where appropriate) and (b) an effect estimate and its precision (for example, confidence or credible interval), ideally using structured tables or plots.", 29, true),
                ("20", null, "RESULTS", "Results of syntheses", "Results of syntheses grouping node.", 30, true),
                ("20a", "20", "RESULTS", "Results of syntheses", "For each synthesis, briefly summarize the characteristics and risk of bias among contributing studies.", 31, true),
                ("20b", "20", "RESULTS", "Results of syntheses", "Present results of all statistical syntheses conducted. If meta-analysis was done, present for each the summary estimate and its precision (for example, confidence or credible interval) and measures of statistical heterogeneity. If comparing groups, describe the direction of the effect.", 32, true),
                ("20c", "20", "RESULTS", "Results of syntheses", "Present results of all investigations of possible causes of heterogeneity among study results.", 33, true),
                ("20d", "20", "RESULTS", "Results of syntheses", "Present results of all sensitivity analyses conducted to assess the robustness of the synthesized results.", 34, true),
                ("21", null, "RESULTS", "Reporting biases", "Present assessments of risk of bias due to missing results (arising from reporting biases) for each synthesis assessed.", 35, true),
                ("22", null, "RESULTS", "Certainty of evidence", "Present assessments of certainty (or confidence) in the body of evidence for each outcome assessed.", 36, true),
                ("23", null, "DISCUSSION", "Discussion", "Discussion grouping node.", 37, true),
                ("23a", "23", "DISCUSSION", "Discussion", "Provide a general interpretation of the results in the context of other evidence.", 38, true),
                ("23b", "23", "DISCUSSION", "Discussion", "Discuss any limitations of the evidence included in the review.", 39, true),
                ("23c", "23", "DISCUSSION", "Discussion", "Discuss any limitations of the review processes used.", 40, true),
                ("23d", "23", "DISCUSSION", "Discussion", "Discuss implications of the results for practice, policy, and future research.", 41, true),
                ("24", null, "OTHER_INFORMATION", "Registration and protocol", "Registration and protocol grouping node.", 42, true),
                ("24a", "24", "OTHER_INFORMATION", "Registration and protocol", "Provide registration information for the review, including register name and registration number, or state that the review was not registered.", 43, true),
                ("24b", "24", "OTHER_INFORMATION", "Registration and protocol", "Indicate where the review protocol can be accessed, or state that a protocol was not prepared.", 44, true),
                ("24c", "24", "OTHER_INFORMATION", "Registration and protocol", "Describe and explain any amendments to information provided at registration or in the protocol.", 45, true),
                ("25", null, "OTHER_INFORMATION", "Support", "Describe sources of financial or non-financial support for the review, and the role of the funders or sponsors in the review.", 46, true),
                ("26", null, "OTHER_INFORMATION", "Competing interests", "Declare any competing interests of review authors.", 47, true),
                ("27", null, "OTHER_INFORMATION", "Availability of data, code and other materials", "Report which of the following are publicly available and where they can be found: template data collection forms; data extracted from included studies; data used for all analyses; analytic code; any other materials used in the review.", 48, true)
            };

            var abstractItems = new List<(string Number, string? Parent, string Section, string Topic, string Description, int Order, bool IsRequired)>
            {
                ("1", null, "TITLE", "Title", "Identify the report as a systematic review.", 1, true),
                ("2", null, "BACKGROUND", "Objectives", "Provide an explicit statement of the main objective(s) or question(s) the review addresses.", 2, true),
                ("3", null, "METHODS", "Eligibility criteria", "Specify the inclusion and exclusion criteria for the review.", 3, true),
                ("4", null, "METHODS", "Information sources", "Specify the information sources (for example, databases, registers) used to identify studies and the date when each was last searched.", 4, true),
                ("5", null, "METHODS", "Risk of bias", "Specify the methods used to assess risk of bias in the included studies.", 5, true),
                ("6", null, "METHODS", "Synthesis of results", "Specify the methods used to present and synthesise results.", 6, true),
                ("7", null, "RESULTS", "Included studies", "Give the total number of included studies and participants and summarise relevant characteristics of studies.", 7, true),
                ("8", null, "RESULTS", "Synthesis of results", "Present results for main outcomes, preferably indicating the number of included studies and participants for each. If meta-analysis was done, report the summary estimate and confidence or credible interval.", 8, true),
                ("9", null, "DISCUSSION", "Limitations of evidence", "Provide a brief summary of limitations of the evidence included in the review (for example, study risk of bias, inconsistency and imprecision).", 9, true),
                ("10", null, "DISCUSSION", "Interpretation", "Provide a general interpretation of the results and important implications.", 10, true),
                ("11", null, "OTHER_INFORMATION", "Funding", "Specify the primary source of funding for the review.", 11, true),
                ("12", null, "OTHER_INFORMATION", "Registration", "Provide the register name and registration number.", 12, true)
            };

            var (mainSections, mainSeededItems) = BuildChecklistTemplateStructure(mainTemplate.Id, ChecklistType.Full, mainItems, now);
            var (abstractSections, abstractSeededItems) = BuildChecklistTemplateStructure(abstractTemplate.Id, ChecklistType.Abstract, abstractItems, now);

            await context.ChecklistSectionTemplates.AddRangeAsync(mainSections);
            await context.ChecklistSectionTemplates.AddRangeAsync(abstractSections);
            await context.ChecklistItemTemplates.AddRangeAsync(mainSeededItems);
            await context.ChecklistItemTemplates.AddRangeAsync(abstractSeededItems);
            await context.SaveChangesAsync();
        }

        private static (List<ChecklistSectionTemplate> Sections, List<ChecklistItemTemplate> Items) BuildChecklistTemplateStructure(
            Guid templateId,
            ChecklistType templateType,
            List<(string Number, string? Parent, string Section, string Topic, string Description, int Order, bool IsRequired)> source,
            DateTimeOffset now)
        {
            var sections = new List<ChecklistSectionTemplate>();
            var sectionByKey = new Dictionary<string, ChecklistSectionTemplate>(StringComparer.OrdinalIgnoreCase);
            var sectionIndex = 1;

            foreach (var sectionKey in source
                .OrderBy(x => x.Order)
                .Select(x => x.Section)
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var normalizedName = NormalizeSectionName(sectionKey);
                var section = new ChecklistSectionTemplate
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templateId,
                    Name = normalizedName,
                    Description = null,
                    Order = sectionIndex,
                    SectionNumber = sectionIndex.ToString(),
                    CreatedAt = now,
                    ModifiedAt = now
                };

                sections.Add(section);
                sectionByKey[sectionKey] = section;
                sectionIndex++;
            }

            var items = new List<ChecklistItemTemplate>();
            var byNumber = new Dictionary<string, ChecklistItemTemplate>(StringComparer.OrdinalIgnoreCase);

            // Create all items first in deterministic order so parent lookup can be resolved in a second pass.
            foreach (var item in source.OrderBy(x => x.Order).ThenBy(x => x.Number, StringComparer.OrdinalIgnoreCase))
            {
                if (!sectionByKey.TryGetValue(item.Section, out var section))
                {
                    throw new InvalidOperationException($"Section '{item.Section}' not found for checklist item '{item.Number}' in template '{templateId}'.");
                }

                var entity = new ChecklistItemTemplate
                {
                    Id = Guid.NewGuid(),
                    TemplateId = templateId,
                    SectionId = section.Id,
                    ParentId = null,
                    ItemNumber = item.Number,
                    Section = section.Name,
                    Topic = item.Topic,
                    Description = item.Description,
                    Order = item.Order,
                    IsRequired = item.IsRequired,
                    HasLocationField = templateType == ChecklistType.Full,
                    IsSectionHeaderOnly = false,
                    DefaultSampleAnswer = null,
                    CreatedAt = now,
                    ModifiedAt = now
                };

                items.Add(entity);
                byNumber[item.Number] = entity;
            }

            foreach (var item in source)
            {
                if (string.IsNullOrWhiteSpace(item.Parent))
                {
                    continue;
                }

                if (!byNumber.TryGetValue(item.Number, out var current))
                {
                    throw new InvalidOperationException($"Invalid checklist item number '{item.Number}' while building template '{templateId}'.");
                }

                if (!byNumber.TryGetValue(item.Parent, out var parent))
                {
                    throw new InvalidOperationException($"Parent item '{item.Parent}' not found for checklist item '{item.Number}' in template '{templateId}'.");
                }

                current.ParentId = parent.Id;
            }

            var parentIds = items
                .Where(x => x.ParentId.HasValue)
                .Select(x => x.ParentId!.Value)
                .ToHashSet();

            foreach (var item in items)
            {
                if (parentIds.Contains(item.Id))
                {
                    item.IsSectionHeaderOnly = true;
                    item.HasLocationField = false;
                }
            }

            return (sections, items);
        }

        private static string NormalizeSectionName(string section)
        {
            return section.Trim().ToUpperInvariant() switch
            {
                "TITLE" => "Title",
                "ABSTRACT" => "Abstract",
                "BACKGROUND" => "Background",
                "INTRODUCTION" => "Introduction",
                "METHODS" => "Methods",
                "RESULTS" => "Results",
                "DISCUSSION" => "Discussion",
                "OTHER_INFORMATION" => "Other information",
                _ => section.Replace('_', ' ').Trim()
            };
        }

        private static async Task SeedAuditLogsAsync(AppDbContext context)
        {
            if (await context.AuditLogs.AnyAsync())
            {
                return;
            }

            var auditLogs = new List<AuditLog>
            {
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = AdminUserId.ToString(),
                    ActionType = "Update",
                    ResourceType = "systematic_review_projects",
                    ResourceId = HarProjectId.ToString(),
                    OldValue = "{\"Title\":\"Old Title\"}",
                    NewValue = "{\"Title\":\"AI-based Human Activity Recognition\"}",
                    AffectedColumns = "[\"Title\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-5),
                    ProjectId = HarProjectId
                },
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = AdminUserId.ToString(),
                    ActionType = "Delete",
                    ResourceType = "papers",
                    ResourceId = Guid.NewGuid().ToString(),
                    OldValue = "{\"Title\":\"Some invalid deleted paper\"}",
                    NewValue = "",
                    AffectedColumns = "[\"Title\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-2),
                    ProjectId = HarProjectId
                },

                // --- ScientificLiteracyProject Actions ---
                // 1. Project Creation
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = AdminUserId.ToString(),
                    ActionType = "Create",
                    ResourceType = "systematic_review_projects",
                    ResourceId = ScientificLiteracyProjectId.ToString(),
                    OldValue = "",
                    NewValue = JsonSerializer.Serialize(new
                    {
                        Id = ScientificLiteracyProjectId,
                        Title = "Empowering Scientific Literacy of Science Teachers: A Systematic Literature Review",
                        Domain = "Science Education",
                        Status = "Active"
                    }),
                    AffectedColumns = "[\"Id\", \"Title\", \"Domain\", \"Status\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-30),
                    ProjectId = ScientificLiteracyProjectId
                },
                // 2. Member Adding
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = AdminUserId.ToString(),
                    ActionType = "Create",
                    ResourceType = "project_members",
                    ResourceId = Guid.NewGuid().ToString(),
                    OldValue = "",
                    NewValue = JsonSerializer.Serialize(new
                    {
                        ProjectId = ScientificLiteracyProjectId,
                        UserId = ClientUserId,
                        Role = "Reviewer"
                    }),
                    AffectedColumns = "[\"ProjectId\", \"UserId\", \"Role\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-28),
                    ProjectId = ScientificLiteracyProjectId
                },
                // 3. Phase Change (e.g. Planning -> Identification)
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = AdminUserId.ToString(),
                    ActionType = "Update",
                    ResourceType = "review_processes",
                    ResourceId = Guid.NewGuid().ToString(),
                    OldValue = "{\"CurrentPhase\":\"Planning\"}",
                    NewValue = "{\"CurrentPhase\":\"Identification\"}",
                    AffectedColumns = "[\"CurrentPhase\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-15),
                    ProjectId = ScientificLiteracyProjectId
                },
                // 4. Add Paper
                new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = ClientUserId.ToString(),
                    ActionType = "Create",
                    ResourceType = "papers",
                    ResourceId = Guid.NewGuid().ToString(),
                    OldValue = "",
                    NewValue = JsonSerializer.Serialize(new
                    {
                        Title = "New insights into science education literacy",
                        PublicationYear = "2023",
                        ProjectId = ScientificLiteracyProjectId
                    }),
                    AffectedColumns = "[\"Title\", \"PublicationYear\", \"ProjectId\"]",
                    Timestamp = DateTime.UtcNow.AddDays(-5),
                    ProjectId = ScientificLiteracyProjectId
                }
            };

            context.AuditLogs.AddRange(auditLogs);
            await context.SaveChangesAsync();
        }
    }
}