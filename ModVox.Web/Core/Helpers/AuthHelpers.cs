namespace ModVox.Web.Security;

public static class AuthHelpers
{
    public static string? TryGetBearerToken(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return null;
        }

        const string prefix = "Bearer ";
        var headerValue = authorizationHeader.ToString();
        if (!headerValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return headerValue[prefix.Length..].Trim();
    }
}
