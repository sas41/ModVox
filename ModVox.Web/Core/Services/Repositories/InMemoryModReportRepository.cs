using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryModReportRepository : IModReportRepository
{
    private readonly ConcurrentDictionary<Guid, ModReportRecord> _reports = new();

    public Task AddAsync(ModReportRecord report, CancellationToken cancellationToken)
    {
        _reports[report.Id] = report;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ModReportRecord>> ListOpenAsync(CancellationToken cancellationToken)
    {
        var reports = _reports.Values
            .Where(x => string.Equals(x.Status, ModReportStatus.Open, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ModReportRecord>>(reports);
    }

    public Task<ModReportRecord?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken)
    {
        _reports.TryGetValue(reportId, out var report);
        return Task.FromResult(report);
    }

    public Task UpdateAsync(ModReportRecord report, CancellationToken cancellationToken)
    {
        _reports[report.Id] = report;
        return Task.CompletedTask;
    }
}
