namespace ModVox.Web.Domain;

public sealed record ModReportRecord(
    Guid Id,
    Guid ModId,
    Guid ReporterUserId,
    string ReportType,
    string Details,
    string Status,
    Guid? ResolvedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNote);

public static class ModReportType
{
    public const string RuleViolation = "rule_violation";
    public const string MaliciousCode = "malicious_code";
    public const string NotWorking = "not_working";

    public static bool IsAllowed(string value)
    {
        return string.Equals(value, RuleViolation, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, MaliciousCode, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, NotWorking, StringComparison.OrdinalIgnoreCase);
    }
}

public static class ModReportStatus
{
    public const string Open = "open";
    public const string Resolved = "resolved";
}
