namespace ModVox.Web.ApiModels;

public sealed record ModReportItemResponse(
    Guid ReportId,
    Guid ModId,
    Guid ReporterUserId,
    string ReportType,
    string Details,
    string Status,
    DateTimeOffset CreatedAt);
