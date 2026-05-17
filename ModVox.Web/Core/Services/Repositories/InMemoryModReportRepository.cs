using System.Collections.Concurrent;
using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public sealed class InMemoryModReportRepository : IModReportRepository
{
    private readonly ConcurrentDictionary<Guid, ModReport> _reports = new();

    public Task AddAsync(ModReport report, CancellationToken cancellationToken)
    {
        _reports[report.Id] = report;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ModReport>> ListOpenAsync(CancellationToken cancellationToken)
    {
        var reports = _reports.Values
            .Where(x => string.Equals(x.Status, ModReportStatus.Open, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.CreatedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ModReport>>(reports);
    }

    public Task<ModReport?> GetByIdAsync(Guid reportId, CancellationToken cancellationToken)
    {
        _reports.TryGetValue(reportId, out var report);
        return Task.FromResult(report);
    }

    public Task UpdateAsync(ModReport report, CancellationToken cancellationToken)
    {
        _reports[report.Id] = report;
        return Task.CompletedTask;
    }
}
