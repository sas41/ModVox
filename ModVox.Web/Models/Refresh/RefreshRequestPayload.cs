namespace ModVox.Web.Refresh;

public sealed class RefreshRequestPayload
{
    public string? IdempotencyKey { get; init; }
}
