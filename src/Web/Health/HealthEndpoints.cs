using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Snail.MCP.AfterEffects.Application.Diagnostics;

namespace Snail.MCP.AfterEffects.Web.Health;

/// <summary>The liveness answer, open to anyone: it says only that the server runs and whether After Effects answers it.</summary>
internal static class HealthEndpoints
{
    public const string Path = "/healthz";

    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Path, (ServerHealth health, CancellationToken cancellationToken) => health.LivenessAsync(cancellationToken));

        return endpoints;
    }
}
