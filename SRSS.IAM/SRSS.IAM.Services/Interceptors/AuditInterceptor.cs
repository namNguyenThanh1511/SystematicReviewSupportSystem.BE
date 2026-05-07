using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Entities.BaseEntity;
using Shared.Repositories;
using SRSS.IAM.Repositories.AuditLogRepo;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.UserService;

namespace SRSS.IAM.Services.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        private readonly AsyncLocal<List<AuditEntry>?> _auditEntries = new();
        private static readonly AsyncLocal<HashSet<string>?> _ignoredTables = new();

        // Maps sub-process FK property names to their parent process entity types
        // so we can look up ReviewProcessId from the parent
        private static readonly Dictionary<string, Type> SubProcessFkToParentType = new()
        {
            { "StudySelectionProcessId", typeof(StudySelectionProcess) },
            { "QualityAssessmentProcessId", typeof(QualityAssessmentProcess) },
            { "DataExtractionProcessId", typeof(DataExtractionProcess) },
            { "SynthesisProcessId", typeof(SynthesisProcess) },
            { "IdentificationProcessId", typeof(IdentificationProcess) },
        };

        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public static void IgnoreTable(string tableName)
        {
            if (_ignoredTables.Value == null)
            {
                _ignoredTables.Value = new HashSet<string>();
            }
            _ignoredTables.Value.Add(tableName);

            // This track is to make sure static class in dotnet behave like scoped service, which means the ignored tables are only applied to the current scope
            if (_ignoredTables.Value.Contains(tableName))
            {
                throw new InvalidOperationException("Too many tables are ignored for auditing. This might indicate a potential issue.");
            }
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;
            if (dbContext == null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var userId = _currentUserService.GetUserId();
            // Only audit user actions (e.g. via API), skip if no user context (e.g. background tasks, seeding)
            if (string.IsNullOrEmpty(userId))
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            var date = DateTime.UtcNow;
            var auditEntries = new List<AuditEntry>();
            var ignoredTableList = _ignoredTables.Value;

            foreach (var entry in dbContext.ChangeTracker.Entries<IBaseEntity>())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.Entity is AuditLog)
                    continue;

                var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
                if (ignoredTableList != null && ignoredTableList.Contains(tableName))
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    ResourceType = tableName
                };

                auditEntries.Add(auditEntry);

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;

                    string propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.ResourceId = property.CurrentValue?.ToString() ?? string.Empty;
                        continue;
                    }

                    // if (propertyName == "CreatedAt" || propertyName == "ModifiedAt")
                    // {
                    //     continue;
                    // }

                    if (propertyName == "ProjectId" && property.CurrentValue is Guid projectId)
                    {
                        auditEntry.ProjectId = projectId;
                    }

                    // Resolve ReviewProcessId from the entity's own ReviewProcessId property
                    if (propertyName == "ReviewProcessId" && property.CurrentValue is Guid reviewProcessId)
                    {
                        auditEntry.ReviewProcessId = reviewProcessId;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.ActionType = "Create";
                            auditEntry.NewValue[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            auditEntry.ActionType = "Delete";
                            auditEntry.OldValue[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ActionType = "Update";
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.OldValue[propertyName] = property.OriginalValue;
                                auditEntry.NewValue[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                // If the entity IS a ReviewProcess, use its own Id as the ReviewProcessId
                if (entry.Entity is ReviewProcess)
                {
                    auditEntry.ReviewProcessId = Guid.TryParse(auditEntry.ResourceId, out var rpId) ? rpId : null;
                }

                // If ReviewProcessId was not resolved yet (child entity without direct ReviewProcessId),
                // try to resolve it via sub-process FK navigation
                if (auditEntry.ReviewProcessId == null)
                {
                    TryResolveReviewProcessIdFromSubProcessFk(entry, auditEntry, dbContext);
                }
            }

            if (auditEntries.Any())
            {
                _auditEntries.Value = auditEntries;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// For child entities (e.g., ScreeningDecision -> StudySelectionProcessId,
        /// QualityAssessmentDecision -> QualityAssessmentProcessId, etc.),
        /// resolve ReviewProcessId by looking up the parent sub-process entity in the DbContext.
        /// </summary>
        private void TryResolveReviewProcessIdFromSubProcessFk(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            AuditEntry auditEntry,
            DbContext dbContext)
        {
            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;

                if (!SubProcessFkToParentType.TryGetValue(propertyName, out var parentType))
                    continue;

                var fkValue = property.CurrentValue ?? property.OriginalValue;
                if (fkValue is not Guid subProcessId)
                    continue;

                // Look up the parent sub-process entity in the change tracker first (avoid DB hit)
                var parentEntry = dbContext.ChangeTracker.Entries()
                    .FirstOrDefault(e => e.Entity.GetType() == parentType && HasMatchingId(e, subProcessId));

                if (parentEntry != null)
                {
                    var reviewProcessIdProp = parentEntry.Properties
                        .FirstOrDefault(p => p.Metadata.Name == "ReviewProcessId");
                    if (reviewProcessIdProp?.CurrentValue is Guid rpId)
                    {
                        auditEntry.ReviewProcessId = rpId;
                        return;
                    }
                }

                // If not in change tracker, query from database
                var reviewProcessId = ResolveReviewProcessIdFromDb(dbContext, parentType, subProcessId);
                if (reviewProcessId.HasValue)
                {
                    auditEntry.ReviewProcessId = reviewProcessId.Value;
                    return;
                }
            }
        }

        private static bool HasMatchingId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Guid targetId)
        {
            var pkProp = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return pkProp?.CurrentValue is Guid id && id == targetId;
        }

        private static Guid? ResolveReviewProcessIdFromDb(DbContext dbContext, Type parentType, Guid subProcessId)
        {
            // Use raw SQL or switch on type to query the ReviewProcessId
            if (parentType == typeof(StudySelectionProcess))
            {
                return dbContext.Set<StudySelectionProcess>()
                    .Where(e => e.Id == subProcessId)
                    .Select(e => (Guid?)e.ReviewProcessId)
                    .FirstOrDefault();
            }
            if (parentType == typeof(QualityAssessmentProcess))
            {
                return dbContext.Set<QualityAssessmentProcess>()
                    .Where(e => e.Id == subProcessId)
                    .Select(e => (Guid?)e.ReviewProcessId)
                    .FirstOrDefault();
            }
            if (parentType == typeof(DataExtractionProcess))
            {
                return dbContext.Set<DataExtractionProcess>()
                    .Where(e => e.Id == subProcessId)
                    .Select(e => (Guid?)e.ReviewProcessId)
                    .FirstOrDefault();
            }
            if (parentType == typeof(SynthesisProcess))
            {
                return dbContext.Set<SynthesisProcess>()
                    .Where(e => e.Id == subProcessId)
                    .Select(e => (Guid?)e.ReviewProcessId)
                    .FirstOrDefault();
            }
            if (parentType == typeof(IdentificationProcess))
            {
                return dbContext.Set<IdentificationProcess>()
                    .Where(e => e.Id == subProcessId)
                    .Select(e => (Guid?)e.ReviewProcessId)
                    .FirstOrDefault();
            }

            return null;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var auditEntries = _auditEntries.Value;
            var dbContext = eventData.Context;

            if (auditEntries != null && auditEntries.Any() && dbContext != null)
            {
                var userId = _currentUserService.GetUserId();
                var userName = _currentUserService.GetUserName();

                foreach (var auditEntry in auditEntries)
                {
                    // For added entries, primary keys are now generated
                    foreach (var prop in auditEntry.Entry.Properties)
                    {
                        if (prop.Metadata.IsPrimaryKey() && string.IsNullOrEmpty(auditEntry.ResourceId))
                        {
                            auditEntry.ResourceId = prop.CurrentValue?.ToString() ?? string.Empty;
                        }
                    }

                    dbContext.Set<AuditLog>().Add(auditEntry.ToAuditLog(userId, userName));
                }

                // Clear to prevent loops since we are calling SaveChangesAsync again
                _auditEntries.Value = null;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
