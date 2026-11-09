using Microsoft.AspNetCore.Http;
using Snail.MCP.AfterEffects.Extensions;

namespace Snail.MCP.AfterEffects.Web.Access;

/// <summary>The caller of an HTTP request: the holder of the key it presented, and the scopes on that key.</summary>
/// <remarks>Only the configured token lets its holder name itself with the agent header; a client key is the name on it.</remarks>
internal sealed class RequestIdentity(IHttpContextAccessor requests) : IClientIdentity
{
    public ClientIdentity? Who() =>
        requests.HttpContext is { } context ? new ClientIdentity(Whom(context), Opens(context)) : null;

    /// <summary>The kinds of work the key this request presented opens; a request admitted some other way opens all of them.</summary>
    private static IReadOnlyList<string>? Opens(HttpContext context) =>
        context.User.FindAll(WebAccess.ScopeClaim).Select(claim => claim.Value).ToList() is { Count: > 0 } scopes ? scopes : null;

    private static string? Whom(HttpContext context) =>
        context.User.FindFirst(WebAccess.ConfiguredClaim) is null
            ? context.User.Identity?.Name
            : context.Request.Headers[TransportRegistrationExtensions.AgentHeader].ToString() is { Length: > 0 } named ? named : context.User.Identity?.Name;
}
