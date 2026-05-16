using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IModReportRepository
{
    Task AddAsync(ModReportRecord report, CancellationToken cancellationToken);
    Task<IReadOnlyList<ModReportRecord>> ListOpenAsync(CancellationToken cancellationToken);
    Task<ModReportRecord?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken);
    Task UpdateAsync(ModReportRecord report, CancellationToken cancellationToken);
}
