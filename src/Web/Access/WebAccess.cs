namespace Snail.MCP.AfterEffects.Web.Access;

/// <summary>The ways into the HTTP server and who each one admits.</summary>
internal static class WebAccess
{
    /// <summary>A client sending its key as a bearer on every request: Claude Code on <c>/mcp</c>, or a script on the files.</summary>
    public const string BearerScheme = "Bearer";

    /// <summary>One kind of work the admitted client's key opens; a ticket carries one claim of these per scope.</summary>
    public const string ScopeClaim = "snail:scope";

    /// <summary>Marks the ticket of the token the server was configured with, the one key that is not a client of its own: whoever holds it may still name itself.</summary>
    public const string ConfiguredClaim = "snail:configured";

    /// <summary>A request carrying a signed link to exactly the file it asks for.</summary>
    public const string LinkScheme = "Link";

    /// <summary>A browser that signed in on the login page with a key and keeps a session cookie.</summary>
    public const string CookieScheme = "Session";

    /// <summary>The pages are for a signed-in browser; anything else is sent to sign in.</summary>
    public const string BrowserPolicy = "browser";

    /// <summary>Reading a file is also open to a signed-in browser, for previews and downloads from the pages; writing never is.</summary>
    public const string FilesReadPolicy = "files-read";

    /// <summary>Only a client holding a key reaches the MCP endpoint.</summary>
    public const string ClientPolicy = "client";

    /// <summary>Writing a file takes a key that opens writing, or a link signed for exactly that write.</summary>
    public const string FilesWritePolicy = "files-write";

    /// <summary>Deleting a file takes a key that opens losing work, or a link signed for exactly that delete.</summary>
    public const string FilesDeletePolicy = "files-delete";

    /// <summary>A page's own form that deletes; the browser's session carries the scopes of the key it signed in with.</summary>
    public const string BrowserDeletePolicy = "browser-delete";

    /// <summary>A page's own form that cancels a render job.</summary>
    public const string BrowserFarmPolicy = "browser-farm";
}
