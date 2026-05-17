using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfAccountSessionRepository : IAccountSessionRepository
{
    private readonly ModVoxDbContext _db;
    public EfAccountSessionRepository(ModVoxDbContext db) => _db = db;

    public async Task AddAsync(AccountSession session, CancellationToken cancellationToken)
    {
        _db.AccountSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<AccountSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken)
        => await _db.AccountSessions.AsNoTracking().FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

    public async Task DeleteAsync(string sessionId, CancellationToken cancellationToken)
    {
        await _db.AccountSessions.Where(s => s.SessionId == sessionId).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _db.AccountSessions.Where(s => s.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }
}
