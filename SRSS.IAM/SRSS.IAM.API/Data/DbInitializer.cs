using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SRSS.IAM.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.API.Data
{
    public static class DbInitializer
    {
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
            if (context.SystematicReviewProjects.Any())
            {
                return;
            }

            var project = new SystematicReviewProject
            {
                Id = Guid.NewGuid(),
                Title = "AI-based Human Activity Recognition",
                Domain = "Computer Science",
                Description = "Systematic review of AI-based HAR approaches using deep learning and machine learning techniques",
                Status = ProjectStatus.Active,
                StartDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.SystematicReviewProjects.AddAsync(project);
            await context.SaveChangesAsync();
        }

        private static async Task SeedReviewProcessesAsync(AppDbContext context)
        {
            if (context.ReviewProcesses.Any())
            {
                return;
            }

            var project = await context.SystematicReviewProjects.FirstOrDefaultAsync();
            if (project == null)
            {
                return;
            }

            var reviewProcess = new ReviewProcess
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
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
            if (context.IdentificationProcesses.Any())
            {
                return;
            }

            var reviewProcess = await context.ReviewProcesses.FirstOrDefaultAsync();
            if (reviewProcess == null)
            {
                return;
            }

            var identificationProcess = new IdentificationProcess
            {
                Id = Guid.NewGuid(),
                ReviewProcessId = reviewProcess.Id,
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
            if (context.SearchExecutions.Any())
            {
                return;
            }

            var identificationProcess = await context.IdentificationProcesses.FirstOrDefaultAsync();
            if (identificationProcess == null)
            {
                return;
            }

            var searchExecution = new SearchExecution
            {
                Id = Guid.NewGuid(),
                IdentificationProcessId = identificationProcess.Id,
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
            if (context.ImportBatches.Any())
            {
                return;
            }

            var searchExecution = await context.SearchExecutions.FirstOrDefaultAsync();
            if (searchExecution == null)
            {
                return;
            }

            var importBatch = new ImportBatch
            {
                Id = Guid.NewGuid(),
                FileName = "scopus_har_results.ris",
                FileType = "RIS",
                Source = "Scopus",
                ImportedBy = "demo_user",
                ImportedAt = DateTimeOffset.UtcNow,
                TotalRecords = 3,
                SearchExecutionId = searchExecution.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            await context.ImportBatches.AddAsync(importBatch);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPapersAsync(AppDbContext context)
        {
            if (context.Papers.Any())
            {
                return;
            }

            var importBatch = await context.ImportBatches.FirstOrDefaultAsync();
            var project = await context.SystematicReviewProjects.FirstOrDefaultAsync();

            if (importBatch == null || project == null)
            {
                return;
            }

            var papers = new List<Paper>
            {
                new Paper
                {
                    Id = Guid.NewGuid(),
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
                    ProjectId = project.Id,
                    ImportBatchId = importBatch.Id,
                    Source = "RIS",
                    ImportedAt = DateTimeOffset.UtcNow,
                    ImportedBy = "demo_user",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new Paper
                {
                    Id = Guid.NewGuid(),
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
                    ProjectId = project.Id,
                    ImportBatchId = importBatch.Id,
                    Source = "RIS",
                    ImportedAt = DateTimeOffset.UtcNow,
                    ImportedBy = "demo_user",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
                new Paper
                {
                    Id = Guid.NewGuid(),
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
                    ProjectId = project.Id,
                    ImportBatchId = importBatch.Id,
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
            var project = await context.SystematicReviewProjects.FirstOrDefaultAsync();
            if (project == null) return;

            // ── ReviewNeed ──────────────────────────────────────────────────
            if (!context.ReviewNeeds.Any())
            {
                var reviewNeeds = new List<ReviewNeed>
                {
                    new ReviewNeed
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        Description = "Systematic review needed to evaluate deep learning approaches for human activity recognition using wearable sensors",
                        Justification = "Current literature lacks a comprehensive comparison of DL architectures under real-world conditions with limited labelled data",
                        IdentifiedBy = "Dr. Nguyen",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ReviewNeed
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
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
            if (!context.CommissioningDocuments.Any())
            {
                var commissioningDocument = new CommissioningDocument
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
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
            if (!context.ReviewObjectives.Any())
            {
                var reviewObjectives = new List<ReviewObjective>
                {
                    new ReviewObjective
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        ObjectiveStatement = "To identify and compare deep learning architectures used for human activity recognition with wearable sensor data, assessing their accuracy, computational cost, and generalisation capability",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ReviewObjective
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        ObjectiveStatement = "To evaluate the impact of data augmentation and transfer learning strategies on HAR model performance across different subject populations",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.ReviewObjectives.AddRangeAsync(reviewObjectives);
                await context.SaveChangesAsync();
            }

            // ── QuestionType ─────────────────────────────────────────────────
            if (!context.QuestionTypes.Any())
            {
                var questionTypes = new List<QuestionType>
                {
                    new QuestionType
                    {
                        Id = Guid.NewGuid(),
                        Name = "PICOC",
                        Description = "Population, Intervention, Comparison, Outcome, Context – standard SLR question framework",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new QuestionType
                    {
                        Id = Guid.NewGuid(),
                        Name = "PICO",
                        Description = "Population, Intervention, Comparison, Outcome – clinical research question framework without explicit context",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new QuestionType
                    {
                        Id = Guid.NewGuid(),
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
            if (!context.ResearchQuestions.Any())
            {
                var picocType = await context.QuestionTypes.FirstOrDefaultAsync(q => q.Name == "PICOC");
                if (picocType == null) return;

                var rq1Id = Guid.NewGuid();
                var rq2Id = Guid.NewGuid();

                var researchQuestions = new List<ResearchQuestion>
                {
                    new ResearchQuestion
                    {
                        Id = rq1Id,
                        ProjectId = project.Id,
                        QuestionTypeId = picocType.Id,
                        QuestionText = "What deep learning approaches are most effective for human activity recognition using wearable sensors in terms of accuracy and computational efficiency?",
                        Rationale = "Understanding the performance trade-offs of different DL architectures will guide practitioners in selecting appropriate models for resource-constrained devices",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    },
                    new ResearchQuestion
                    {
                        Id = rq2Id,
                        ProjectId = project.Id,
                        QuestionTypeId = picocType.Id,
                        QuestionText = "How does transfer learning affect the generalisation of HAR models across different subjects and activity sets?",
                        Rationale = "Cross-subject generalisation is a known bottleneck; synthesising transfer learning evidence will inform future benchmark design",
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }
                };

                await context.ResearchQuestions.AddRangeAsync(researchQuestions);
                await context.SaveChangesAsync();

                // ── PicocElements for RQ1 ────────────────────────────────────
                var picoc1PopId = Guid.NewGuid();
                var picoc1InvId = Guid.NewGuid();
                var picoc1CmpId = Guid.NewGuid();
                var picoc1OutId = Guid.NewGuid();
                var picoc1CtxId = Guid.NewGuid();

                var picocElementsRq1 = new List<PicocElement>
                {
                    new PicocElement { Id = picoc1PopId, ResearchQuestionId = rq1Id, ElementType = "Population",     Description = "Adults performing daily living activities monitored by wearable IMU sensors", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc1InvId, ResearchQuestionId = rq1Id, ElementType = "Intervention",   Description = "Deep learning models (CNN, LSTM, Transformer) applied to raw sensor time-series data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc1CmpId, ResearchQuestionId = rq1Id, ElementType = "Comparison",     Description = "Traditional machine learning classifiers (SVM, Random Forest, k-NN)", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc1OutId, ResearchQuestionId = rq1Id, ElementType = "Outcome",        Description = "Recognition accuracy (F1-score), inference latency, and model size on benchmark datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc1CtxId, ResearchQuestionId = rq1Id, ElementType = "Context",        Description = "Controlled laboratory and free-living environments using publicly available HAR datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                };

                await context.PicocElements.AddRangeAsync(picocElementsRq1);
                await context.SaveChangesAsync();

                // child detail rows for RQ1 PICOC elements
                await context.Populations.AddAsync(new Population   { Id = Guid.NewGuid(), PicocId = picoc1PopId, Description = "Healthy adults (18–65 years) in both controlled-lab and free-living settings", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Guid.NewGuid(), PicocId = picoc1InvId, Description = "End-to-end deep learning pipelines trained on windowed accelerometer/gyroscope data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison    { Id = Guid.NewGuid(), PicocId = picoc1CmpId, Description = "Hand-crafted feature extraction followed by classical ML classifiers", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome          { Id = Guid.NewGuid(), PicocId = picoc1OutId, Metric = "F1-score, Accuracy, Inference Time (ms)", Description = "Per-activity and macro-average F1-score on UCI HAR, OPPORTUNITY, and PAMAP2 datasets", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context          { Id = Guid.NewGuid(), PicocId = picoc1CtxId, Environment = "Laboratory and free-living", Description = "Studies published in English in peer-reviewed venues between 2015 and 2025", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();

                // ── PicocElements for RQ2 ────────────────────────────────────
                var picoc2PopId = Guid.NewGuid();
                var picoc2InvId = Guid.NewGuid();
                var picoc2CmpId = Guid.NewGuid();
                var picoc2OutId = Guid.NewGuid();
                var picoc2CtxId = Guid.NewGuid();

                var picocElementsRq2 = new List<PicocElement>
                {
                    new PicocElement { Id = picoc2PopId, ResearchQuestionId = rq2Id, ElementType = "Population",     Description = "Heterogeneous subject groups across age, fitness level, and activity repertoire", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc2InvId, ResearchQuestionId = rq2Id, ElementType = "Intervention",   Description = "Pre-trained deep learning models fine-tuned on target subject data (transfer learning)", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc2CmpId, ResearchQuestionId = rq2Id, ElementType = "Comparison",     Description = "Models trained from scratch on target subject data without pre-training", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc2OutId, ResearchQuestionId = rq2Id, ElementType = "Outcome",        Description = "Cross-subject accuracy improvement and training data reduction achieved by transfer learning", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow },
                    new PicocElement { Id = picoc2CtxId, ResearchQuestionId = rq2Id, ElementType = "Context",        Description = "Subject-independent and leave-one-subject-out evaluation protocols", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow }
                };

                await context.PicocElements.AddRangeAsync(picocElementsRq2);
                await context.SaveChangesAsync();

                // child detail rows for RQ2 PICOC elements
                await context.Populations.AddAsync(new Population    { Id = Guid.NewGuid(), PicocId = picoc2PopId, Description = "Subjects with varying demographics in leave-one-subject-out cross-validation splits", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Interventions.AddAsync(new Intervention { Id = Guid.NewGuid(), PicocId = picoc2InvId, Description = "Domain adaptation and fine-tuning strategies using source-domain pre-trained weights", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Comparisons.AddAsync(new Comparison     { Id = Guid.NewGuid(), PicocId = picoc2CmpId, Description = "Baseline models trained exclusively on target subject data", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Outcomes.AddAsync(new Outcome           { Id = Guid.NewGuid(), PicocId = picoc2OutId, Metric = "Accuracy gain (%), Required labelled samples", Description = "Reduction in labelled training samples required to match scratch-trained model accuracy", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.Contexts.AddAsync(new Context           { Id = Guid.NewGuid(), PicocId = picoc2CtxId, Environment = "Cross-subject evaluation", Description = "HAR datasets supporting subject-independent splits: NTU RGB+D, PAMAP2, OPPORTUNITY", CreatedAt = DateTimeOffset.UtcNow, ModifiedAt = DateTimeOffset.UtcNow });
                await context.SaveChangesAsync();
            }
        }
    }
}