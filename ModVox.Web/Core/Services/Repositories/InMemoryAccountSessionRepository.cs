using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryAccountSessionRepository : IAccountSessionRepository
{
    private readonly ConcurrentDictionary<string, AccountSessionRecord> _sessions = new(StringComparer.Ordinal);

    public Task AddAsync(AccountSessionRecord session, CancellationToken cancellationToken)
    {
        _sessions[session.SessionId] = session;
        return Task.CompletedTask;
    }

    public Task<AccountSessionRecord?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task DeleteAsync(string sessionId, CancellationToken cancellationToken)
    {
        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var keys = _sessions
            .Where(x => x.Value.UserId == userId)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keys)
        {
            _sessions.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
