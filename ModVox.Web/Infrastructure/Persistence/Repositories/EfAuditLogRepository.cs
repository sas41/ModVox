using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfAuditLogRepository : IAuditLogRepository
{
    private readonly ModVoxDbContext _db;
    public EfAuditLogRepository(ModVoxDbContext db) => _db = db;

    public async Task AddAsync(AuditLog record, CancellationToken cancellationToken)
    {
        _db.AuditLog.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> ListAsync(CancellationToken cancellationToken)
        => await _db.AuditLog.AsNoTracking().OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task PurgeAsync(CancellationToken cancellationToken)
    {
        await _db.AuditLog.ExecuteDeleteAsync(cancellationToken);
    }
}
