using System;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperPdfRepo
{
    public interface IPaperPdfRepository : IGenericRepository<PaperPdf, Guid, AppDbContext>
    {
        Task<PaperPdf?> GetLatestPaperPdfAsync(Guid paperId, CancellationToken cancellationToken = default);
    }
}
