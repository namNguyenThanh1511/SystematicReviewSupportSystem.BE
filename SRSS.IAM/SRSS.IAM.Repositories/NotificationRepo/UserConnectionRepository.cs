using System.Collections.Concurrent;

namespace SRSS.IAM.Repositories.NotificationRepo
{
    public class UserConnectionRepository : IUserConnectionRepository
    {
        private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _userConnections = new();
        private readonly ConcurrentDictionary<string, Guid> _connectionUsers = new();

        public Task AddConnectionAsync(Guid userId, string connectionId, CancellationToken cancellationToken = default)
        {
            _connectionUsers[connectionId] = userId;
            var connections = _userConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
            connections[connectionId] = 0;

            return Task.CompletedTask;
        }

        public Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            if (_connectionUsers.TryRemove(connectionId, out var userId)
                && _userConnections.TryGetValue(userId, out var connections))
            {
                connections.TryRemove(connectionId, out _);

                if (connections.IsEmpty)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> GetConnectionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
            {
                return Task.FromResult((IReadOnlyCollection<string>)Array.Empty<string>());
            }

            return Task.FromResult((IReadOnlyCollection<string>)connections.Keys.ToList());
        }
    }
}
