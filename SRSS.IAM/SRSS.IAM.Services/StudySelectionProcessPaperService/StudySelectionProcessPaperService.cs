using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Common;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionProcessPaperService
{
    public class StudySelectionProcessPaperService : IStudySelectionProcessPaperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionProcessPaperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SaveFinalIncludedPapersAsync(Guid processId, CancellationToken cancellationToken)
        {
            // 1 & 2. Retrieves ScreeningResolution records and extracts PaperIds
            // Filter: Phase == FullText, FinalDecision == Include
            var finalIncludedPaperIds = await _unitOfWork.ScreeningResolutions.GetResolvedPaperIdsByPhaseAsync(
                processId,
                ScreeningPhase.FullText,
                ScreeningDecisionType.Include,
                cancellationToken);

            // 3. Deletes all existing records in StudySelectionProcessPaper for that StudySelectionProcessId
            await _unitOfWork.StudySelectionProcessPapers.DeleteByProcessAsync(processId, cancellationToken);

            // 4 & 5. Inserts new StudySelectionProcessPaper records with new IDs and UTC timestamps
            var newProcessPapers = finalIncludedPaperIds.Select(paperId => new StudySelectionProcessPaper
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = processId,
                PaperId = paperId,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            if (newProcessPapers.Any())
            {
                await _unitOfWork.StudySelectionProcessPapers.AddRangeAsync(newProcessPapers, cancellationToken);
            }
        }

        public async Task<PaginatedResponse<IncludedPaperResponse>> GetIncludedPapersByProcessIdAsync(Guid processId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            var (includedPapers, totalCount) = await _unitOfWork.StudySelectionProcessPapers.GetWithPaperByProcessAsync(processId, search, pageNumber, pageSize, cancellationToken);

            var items = includedPapers.Select(ip => new IncludedPaperResponse
            {
                PaperId = ip.PaperId,
                Title = ip.Paper.Title,
                DOI = ip.Paper.DOI,
                Authors = ip.Paper.Authors,
                Abstract = ip.Paper.Abstract,
                PublicationYear = ip.Paper.PublicationYear,
                PublicationType = ip.Paper.PublicationType,
                Journal = ip.Paper.Journal,
                Source = ip.Paper.Source,
                Keywords = ip.Paper.Keywords,
                Url = ip.Paper.Url,
                PdfUrl = ip.Paper.PdfUrl,
                CreatedAt = ip.CreatedAt
            }).ToList();

            return new PaginatedResponse<IncludedPaperResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Validates and saves multiple included papers for FullText phase.
        /// Checks that each paper belongs to the study selection process and has an "Included" resolution in FullText phase.
        /// </summary>
        public async Task SaveMultipleIncludedPapersInFullTextPhaseAsync(Guid processId, List<Guid> paperIds, CancellationToken cancellationToken)
        {
            if (paperIds == null || paperIds.Count == 0)
            {
                return;
            }

            // Validate all papers belong to this study selection process and have Included resolution in FullText phase
            var validResolutions = await _unitOfWork.ScreeningResolutions.GetByProcessAsync(processId, ScreeningPhase.FullText, cancellationToken);
            var validPaperIds = validResolutions
                .Where(r => r.FinalDecision == ScreeningDecisionType.Include)
                .Select(r => r.PaperId)
                .ToHashSet();

            var invalidPaperIds = paperIds.Where(id => !validPaperIds.Contains(id)).ToList();
            if (invalidPaperIds.Any())
            {
                throw new InvalidOperationException($"The following papers are not eligible for inclusion: {string.Join(", ", invalidPaperIds)}. " +
                    "Papers must belong to the study selection process and have an 'Included' resolution in the FullText phase.");
            }

            // Fetch existing process papers to update their flags
            var existingProcessPapers = await _unitOfWork.StudySelectionProcessPapers.FindAllAsync(
                ip => ip.StudySelectionProcessId == processId,
                isTracking: true,
                cancellationToken: cancellationToken);

            var inputPaperIds = paperIds.ToHashSet();

            foreach (var pp in existingProcessPapers)
            {
                pp.IsAddedToDataset = inputPaperIds.Contains(pp.PaperId);
                pp.ModifiedAt = DateTimeOffset.UtcNow;
                inputPaperIds.Remove(pp.PaperId); // Handled
            }

            // For papers not in the table yet but in input, add them
            if (inputPaperIds.Any())
            {
                foreach (var paperId in inputPaperIds)
                {
                    await _unitOfWork.StudySelectionProcessPapers.AddAsync(new StudySelectionProcessPaper
                    {
                        Id = Guid.NewGuid(),
                        StudySelectionProcessId = processId,
                        PaperId = paperId,
                        IsAddedToDataset = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ModifiedAt = DateTimeOffset.UtcNow
                    }, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
