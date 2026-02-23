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
        }

        private static async Task SeedProjectsAsync(AppDbContext context)
        {
            if (context.SystematicReviewProjects.Any())
            {
                return;
            }

            var projects = new List<SystematicReviewProject> {
                new SystematicReviewProject{
                    Id = Guid.NewGuid(),
                    Title = "AI-based Human Activity Recognition",
                    Domain = "Computer Science",
                    Description = "Systematic review of AI-based HAR approaches using deep learning and machine learning techniques",
                    Status = ProjectStatus.Active,
                    StartDate = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ModifiedAt = DateTimeOffset.UtcNow
                },
				new SystematicReviewProject{
					Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
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
    }
}