using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.PaperEmbeddingRepo
{
    public class PaperEmbeddingRepository : GenericRepository<PaperEmbedding, Guid, AppDbContext>, IPaperEmbeddingRepository
    {
        public PaperEmbeddingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PaperEmbedding?> FindClosestByCosineDistanceAsync(
            float[] embedding,
            Guid currentPaperId,
            CancellationToken cancellationToken = default,
            int take = 1)
        {
            if (embedding == null || embedding.Length == 0)
            {
                return null;
            }

            var queryVector = new Vector(embedding);

            return await _context.PaperEmbeddings
                .FromSqlInterpolated<PaperEmbedding>($@"
                    SELECT *
                    FROM paper_embeddings
                    WHERE paper_id <> {currentPaperId}
                    ORDER BY embedding <=> {queryVector}
                    LIMIT {Math.Max(1, take)}")
                .AsNoTracking()
                .Include(pe => pe.Paper)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<PaperEmbedding>> FindClosestByCosineDistanceInIdentificationProcessAsync(
            float[] embedding,
            Guid identificationProcessId,
            CancellationToken cancellationToken = default,
            int take = 5)
        {
            if (embedding == null || embedding.Length == 0)
            {
                return new List<PaperEmbedding>();
            }

            var queryVector = new Vector(embedding);
            var safeTake = Math.Max(1, take);

            var projectId = await _context.IdentificationProcesses
                .AsNoTracking()
                .Where(ip => ip.Id == identificationProcessId)
                .Select(ip => ip.ReviewProcess.ProjectId)
                .SingleOrDefaultAsync(cancellationToken);

            if (projectId == Guid.Empty)
            {
                return new List<PaperEmbedding>();
            }

            return await _context.PaperEmbeddings
                .FromSqlInterpolated($@"
                    SELECT pe.*
                    FROM paper_embeddings pe
                    JOIN papers p ON p.id = pe.paper_id
                    JOIN import_batches ib ON ib.id = p.import_batch_id
                    JOIN search_executions se ON se.id = ib.search_execution_id
                    WHERE se.identification_process_id = {identificationProcessId}
                      AND p.is_deleted = false
                      AND NOT EXISTS (
                          SELECT 1
                          FROM deduplication_results dr
                          WHERE dr.paper_id = p.id
                            AND dr.project_id = {projectId}
                            AND dr.review_status = {DeduplicationReviewStatus.Confirmed.ToString()}
                            AND dr.resolved_decision = {DuplicateResolutionDecision.CANCEL.ToString()}
                      )
                    ORDER BY pe.embedding <=> {queryVector}
                    LIMIT {safeTake}")
                .AsNoTracking()
                .Include(pe => pe.Paper)
                .ToListAsync(cancellationToken);
        }
    }
}
