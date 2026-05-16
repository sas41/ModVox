namespace ModVox.Web.ApiModels;

public sealed record ResolveReportResponse(
    Guid ReportId,
    string Status,
    Guid ResolvedByUserId,
    DateTimeOffset ResolvedAt);
