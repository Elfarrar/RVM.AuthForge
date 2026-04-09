using System.Security.Claims;
using System.Text.Encodings.Web;
using RVM.AuthForge.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RVM.AuthForge.API.Auth;

public class ApiKeyAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IServiceScopeFactory scopeFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            return AuthenticateResult.NoResult();

        var apiKey = apiKeyHeader.ToString();
        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("API key is empty.");

        using var scope = scopeFactory.CreateScope();
        var keyService = scope.ServiceProvider.GetRequiredService<IApiKeyService>();
        var key = await keyService.ValidateAsync(apiKey);

        if (key is null)
            return AuthenticateResult.Fail("Invalid API key.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, key.AppId),
            new Claim(ClaimTypes.Name, key.Name),
            new Claim("api_key_id", key.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
