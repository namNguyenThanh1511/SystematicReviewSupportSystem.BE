using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;

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

        // ── Search Execution IDs ─────────────────────────────────────────
        private static readonly Guid ScopusSearchExecutionId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        // ── Import Batch IDs ─────────────────────────────────────────────
        private static readonly Guid ScopusImportBatchId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // ── Paper IDs ────────────────────────────────────────────────────
        private static readonly Guid Paper1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
        private static readonly Guid Paper2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
        private static readonly Guid Paper3Id = Guid.Parse("88888888-8888-8888-8888-888888888888");

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

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedProjectsAsync(context);
            await SeedReviewProcessesAsync(context);
            await SeedIdentificationProcessesAsync(context);
            await SeedSearchExecutionsAsync(context);
            await SeedImportBatchesAsync(context);
            await SeedPapersAsync(context);
            await SeedCoreGovernanceAsync(context);
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

        private static async Task SeedSearchExecutionsAsync(AppDbContext context)
        {
            if (await context.SearchExecutions.AnyAsync(x => x.Id == ScopusSearchExecutionId))
            {
                return;
            }

            var searchExecution = new SearchExecution
            {
                Id = ScopusSearchExecutionId,
                IdentificationProcessId = HarIdentificationProcessId,
                SearchSource = "Scopus",
                SearchQuery = "TITLE-ABS-KEY(\"human activity recognition\" AND \"deep learning\")",
                ExecutedAt = DateTimeOffset.UtcNow,
                ResultCount = 3,
                Type = SearchExecutionType.DatabaseSearch,
                Notes = "Initial Scopus database search for HAR papers",
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.SearchExecutions.AddAsync(searchExecution);
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
                FileName = "scopus_har_results.ris",
                FileType = "RIS",
                Source = "Scopus",
                ImportedBy = "demo_user",
                ImportedAt = DateTimeOffset.UtcNow,
                TotalRecords = 3,
                SearchExecutionId = ScopusSearchExecutionId,
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
                await context.Populations.AddAsync(new Population   { Id = Rq1PopulationDetailId, PicocId = Rq1PopulationId, Description = "Healthy adults (18–65 years) in both controlled-lab and free-living settings", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Rq1InterventionDetailId, PicocId = Rq1InterventionId, Description = "End-to-end deep learning pipelines trained on windowed accelerometer/gyroscope data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison    { Id = Rq1ComparisonDetailId, PicocId = Rq1ComparisonId, Description = "Hand-crafted feature extraction followed by classical ML classifiers", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome          { Id = Rq1OutcomeDetailId, PicocId = Rq1OutcomeId, Metric = "F1-score, Accuracy, Inference Time (ms)", Description = "Per-activity and macro-average F1-score on UCI HAR, OPPORTUNITY, and PAMAP2 datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context          { Id = Rq1ContextDetailId, PicocId = Rq1ContextId, Environment = "Laboratory and free-living", Description = "Studies published in English in peer-reviewed venues between 2015 and 2025", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
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
                await context.Populations.AddAsync(new Population    { Id = Rq2PopulationDetailId, PicocId = Rq2PopulationId, Description = "Subjects with varying demographics in leave-one-subject-out cross-validation splits", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Rq2InterventionDetailId, PicocId = Rq2InterventionId, Description = "Domain adaptation and fine-tuning strategies using source-domain pre-trained weights", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison     { Id = Rq2ComparisonDetailId, PicocId = Rq2ComparisonId, Description = "Baseline models trained exclusively on target subject data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome           { Id = Rq2OutcomeDetailId, PicocId = Rq2OutcomeId, Metric = "Accuracy gain (%), Required labelled samples", Description = "Reduction in labelled training samples required to match scratch-trained model accuracy", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context           { Id = Rq2ContextDetailId, PicocId = Rq2ContextId, Environment = "Cross-subject evaluation", Description = "HAR datasets supporting subject-independent splits: NTU RGB+D, PAMAP2, OPPORTUNITY", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();
            }
        }
    }
}