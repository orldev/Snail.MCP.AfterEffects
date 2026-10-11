using Microsoft.Extensions.Options;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Web.Access;

/// <summary>Stops an HTTP server that has no token before it listens, instead of letting it turn every client away.</summary>
internal sealed class HttpAccessValidation : IValidateOptions<ServerConfig>
{
    public ValidateOptionsResult Validate(string? name, ServerConfig options) =>
        options.Transport is Transport.Http && ClientToken.Read(options.Http) is null
            ? ValidateOptionsResult.Fail(Messages.HttpTokenMissing)
            : ValidateOptionsResult.Success;
}
