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
        public AuditInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
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

            foreach (var entry in dbContext.ChangeTracker.Entries<IBaseEntity>())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                if (entry.Entity is AuditLog)
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    ResourceType = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name
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

                    if (propertyName == "ProjectId" && property.CurrentValue is Guid projectId)
                    {
                        auditEntry.ProjectId = projectId;
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
            }

            if (auditEntries.Any())
            {
                _auditEntries.Value = auditEntries;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
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

                    dbContext.Set<AuditLog>().Add(auditEntry.ToAuditLog(userId));
                }

                // Clear to prevent loops since we are calling SaveChangesAsync again
                _auditEntries.Value = null; 
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
