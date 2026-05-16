using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IAccountSessionRepository
{
    Task AddAsync(AccountSessionRecord session, CancellationToken cancellationToken);
    Task<AccountSessionRecord?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken);
    Task DeleteAsync(string sessionId, CancellationToken cancellationToken);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
