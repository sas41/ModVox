using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<Guid, UserAccount> _users = new();

    public Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<IReadOnlyDictionary<Guid, UserAccount>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var result = userIds
            .Distinct()
            .Where(id => _users.ContainsKey(id))
            .ToDictionary(id => id, id => _users[id]);

        return Task.FromResult<IReadOnlyDictionary<Guid, UserAccount>>(result);
    }

    public Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(x => string.Equals(x.Username, username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task AddAsync(UserAccount user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserAccount user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken)
    {
        var users = _users.Values
            .OrderBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<UserAccount>>(users);
    }
}
