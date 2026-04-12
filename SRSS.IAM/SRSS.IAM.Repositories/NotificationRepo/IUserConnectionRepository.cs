namespace SRSS.IAM.Repositories.NotificationRepo
{
    public interface IUserConnectionRepository
    {
        Task AddConnectionAsync(Guid userId, string connectionId, CancellationToken cancellationToken = default);
        Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<string>> GetConnectionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
