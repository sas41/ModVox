namespace ModVox.Web.ApiModels;

public sealed class CreateModReportRequest
{
    public string ReportType { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}
