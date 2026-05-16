using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("mod_reports")]
public record class ModReport
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("mod_id")]
    public Guid ModId { get; init; }

    [Column("reporter_user_id")]
    public Guid ReporterUserId { get; init; }

    [Column("report_type")]
    [MaxLength(64)]
    [Required]
    public string ReportType { get; init; } = string.Empty;

    [Column("details")]
    [Required]
    public string Details { get; init; } = string.Empty;

    [Column("status")]
    [MaxLength(32)]
    [Required]
    public string Status { get; init; } = ModReportStatus.Open;

    [Column("resolved_by_user_id")]
    public Guid? ResolvedByUserId { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("resolved_at")]
    public DateTimeOffset? ResolvedAt { get; init; }

    [Column("resolution_note")]
    public string? ResolutionNote { get; init; }

    // Navigation properties
    public virtual Mod? Mod { get; protected set; }
    public virtual UserAccount? ReporterUser { get; protected set; }
    public virtual UserAccount? ResolvedByUser { get; protected set; }

    protected ModReport() { }

    public ModReport(
        Guid Id, Guid ModId, Guid ReporterUserId,
        string ReportType, string Details, string Status,
        Guid? ResolvedByUserId, DateTimeOffset CreatedAt,
        DateTimeOffset? ResolvedAt, string? ResolutionNote)
    {
        this.Id = Id; this.ModId = ModId; this.ReporterUserId = ReporterUserId;
        this.ReportType = ReportType; this.Details = Details; this.Status = Status;
        this.ResolvedByUserId = ResolvedByUserId; this.CreatedAt = CreatedAt;
        this.ResolvedAt = ResolvedAt; this.ResolutionNote = ResolutionNote;
    }
}

public static class ModReportType
{
    public const string RuleViolation = "rule_violation";
    public const string MaliciousCode = "malicious_code";
    public const string NotWorking = "not_working";

    public static bool IsAllowed(string value) =>
        string.Equals(value, RuleViolation, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, MaliciousCode, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, NotWorking, StringComparison.OrdinalIgnoreCase);
}

public static class ModReportStatus
{
    public const string Open = "open";
    public const string Resolved = "resolved";
}
