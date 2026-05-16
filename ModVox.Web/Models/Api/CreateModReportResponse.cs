namespace ModVox.Web.ApiModels;

public sealed record CreateModReportResponse(
    Guid ReportId,
    Guid ModId,
    string ReportType,
    string Status);
