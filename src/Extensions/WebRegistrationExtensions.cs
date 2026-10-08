using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Web.Access;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>The HTTP server's own features: who may reach it.</summary>
internal static class WebRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>The client keys checked at start and on every request, and the session a browser signs in to.</summary>
        public IServiceCollection AddWebAccess()
        {
            services.AddSingleton<IValidateOptions<ServerConfig>, HttpAccessValidation>();
            services.TryAddSingleton<ClientToken>();
            services.AddSingleton<ClientKeys>();

            services
                .AddAuthentication(WebAccess.BearerScheme)
                .AddScheme<AuthenticationSchemeOptions, BearerAuthentication>(WebAccess.BearerScheme, configureOptions: null)
                .AddScheme<AuthenticationSchemeOptions, LinkAuthentication>(WebAccess.LinkScheme, configureOptions: null)
                .AddCookie(WebAccess.CookieScheme);

            services
                .AddOptions<CookieAuthenticationOptions>(WebAccess.CookieScheme)
                .Configure<ServerConfig>(Session);

            services
                .AddAuthorizationBuilder()
                .AddPolicy(WebAccess.ClientPolicy, policy => policy.AddAuthenticationSchemes(WebAccess.BearerScheme).RequireAuthenticatedUser())
                .AddPolicy(WebAccess.FilesWritePolicy, policy => policy
                    .AddAuthenticationSchemes(WebAccess.BearerScheme, WebAccess.LinkScheme)
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => Opens(context.User, Scopes.Write)))
                .AddPolicy(WebAccess.FilesDeletePolicy, policy => policy
                    .AddAuthenticationSchemes(WebAccess.BearerScheme, WebAccess.LinkScheme)
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => Opens(context.User, Scopes.Delete)))
                .AddPolicy(WebAccess.FilesReadPolicy, policy => policy
                    .AddAuthenticationSchemes(WebAccess.BearerScheme, WebAccess.LinkScheme, WebAccess.CookieScheme)
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => Opens(context.User, Scopes.Read)))
                .AddPolicy(WebAccess.BrowserPolicy, policy => policy.AddAuthenticationSchemes(WebAccess.CookieScheme).RequireAuthenticatedUser())
                .AddPolicy(WebAccess.BrowserDeletePolicy, policy => policy
                    .AddAuthenticationSchemes(WebAccess.CookieScheme)
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => Opens(context.User, Scopes.Delete)))
                .AddPolicy(WebAccess.BrowserFarmPolicy, policy => policy
                    .AddAuthenticationSchemes(WebAccess.CookieScheme)
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context => Opens(context.User, Scopes.Farm)));

            services.AddAntiforgery();
            services.AddRazorComponents();
            services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

            return services;
        }
    }

    /// <summary>Whether the admitted client's key opens work of this kind; a signed link is the work itself, already granted by the tool that issued it.</summary>
    internal static bool Opens(ClaimsPrincipal user, string scope) =>
        user.Identity?.AuthenticationType == WebAccess.LinkScheme || user.HasClaim(WebAccess.ScopeClaim, scope);

    /// <summary>The session lasts a working day, is sent only to this site, and is checked against the key that opened it on every request.</summary>
    private static void Session(CookieAuthenticationOptions options, ServerConfig config)
    {
        options.Cookie.Name = "snail_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = (config.Http.PublicUrl ?? config.Http.Url).StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.ReturnUrlParameter = "returnUrl";
        options.Events.OnValidatePrincipal = context =>
        {
            var keys = context.HttpContext.RequestServices.GetRequiredService<ClientKeys>();
            var holder = context.Principal?.Identity?.Name;

            if (holder is null || (keys.Configured?.Name != holder && !keys.All.Any(key => !key.IsRevoked && key.Name.Equals(holder, StringComparison.OrdinalIgnoreCase))))
            {
                context.RejectPrincipal();

                return context.HttpContext.SignOutAsync(WebAccess.CookieScheme);
            }

            return Task.CompletedTask;
        };
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/raw"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            }

            context.Response.Redirect(new Uri(context.RedirectUri, UriKind.RelativeOrAbsolute) is { IsAbsoluteUri: true } absolute ? absolute.PathAndQuery : context.RedirectUri);

            return Task.CompletedTask;
        };
    }
}
