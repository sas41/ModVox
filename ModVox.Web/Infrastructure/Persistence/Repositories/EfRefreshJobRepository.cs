using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfRefreshJobRepository : IRefreshJobRepository
{
    private readonly ModVoxDbContext _db;
    public EfRefreshJobRepository(ModVoxDbContext db) => _db = db;

    public async Task AddAsync(RefreshJobRecord job, CancellationToken cancellationToken)
    {
        _db.RefreshJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshJobRecord?> GetByIdAsync(Guid jobId, CancellationToken cancellationToken)
        => await _db.RefreshJobs.FindAsync(new object[] { jobId }, cancellationToken);

    public async Task<RefreshJobRecord?> FindActiveByModAndKeyAsync(Guid modId, string? idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return null;
        return await _db.RefreshJobs.AsNoTracking().FirstOrDefaultAsync(j =>
            j.ModId == modId &&
            j.IdempotencyKey == idempotencyKey &&
            (j.Status == RefreshJobStatus.Queued || j.Status == RefreshJobStatus.Running),
            cancellationToken);
    }

    public async Task UpdateAsync(RefreshJobRecord job, CancellationToken cancellationToken)
    {
        _db.RefreshJobs.Update(job);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
