using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfModReportRepository : IModReportRepository
{
    private readonly ModVoxDbContext _db;
    public EfModReportRepository(ModVoxDbContext db) => _db = db;

    public async Task AddAsync(ModReport report, CancellationToken cancellationToken)
    {
        _db.ModReports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModReport>> ListOpenAsync(CancellationToken cancellationToken)
        => await _db.ModReports.AsNoTracking()
            .Where(r => r.Status == ModReportStatus.Open)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ModReport?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken)
        => await _db.ModReports.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

    public async Task UpdateAsync(ModReport report, CancellationToken cancellationToken)
    {
        _db.ModReports.Update(report);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
