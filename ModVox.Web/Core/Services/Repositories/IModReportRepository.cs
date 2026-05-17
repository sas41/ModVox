using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModReportRepository
{
    Task AddAsync(ModReport report, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModReport>> ListOpenAsync(CancellationToken cancellationToken);
    Task<ModReport?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken);
    Task UpdateAsync(ModReport report, CancellationToken cancellationToken);
}
