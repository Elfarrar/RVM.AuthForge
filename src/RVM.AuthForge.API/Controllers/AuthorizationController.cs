using System.Security.Claims;
using RVM.AuthForge.Domain.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace RVM.AuthForge.API.Controllers;

[ApiController]
public class AuthorizationController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IOpenIddictApplicationManager applicationManager,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var user = await userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var claims = new List<Claim>
        {
            new(Claims.Subject, user.Id.ToString()),
            new(Claims.Email, user.Email!),
            new(Claims.Name, user.FullName)
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(Claims.Role, role));

        var identity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        principal.SetScopes(request.GetScopes());
        principal.SetResources("authforge-api");

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [HttpPost("~/api/internal/auth/phone-login")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var user = await userManager.FindByIdAsync(result.Principal!.GetClaim(Claims.Subject)!);

            if (user is null || !user.Active)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            var identity = new ClaimsIdentity(result.Principal!.Claims,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            identity.SetClaim(Claims.Subject, user.Id.ToString());
            identity.SetClaim(Claims.Email, user.Email);
            identity.SetClaim(Claims.Name, user.FullName);

            var roles = await userManager.GetRolesAsync(user);
            identity.SetClaims(Claims.Role, [.. roles]);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources("authforge-api");

            foreach (var claim in principal.Claims)
                claim.SetDestinations(GetDestinations(claim, principal));

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.SetClaim(Claims.Subject, request.ClientId);

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources("authforge-api");

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.GrantType == "phone_login")
        {
            var apiKey = Request.Headers["X-RVM-Internal-Key"].ToString();
            var expectedKey = configuration["InternalAuth:ApiKey"] ?? "";
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != expectedKey)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid API key."
                    }));
            }

            var clientId = request.ClientId;
            var clientSecret = request.ClientSecret;

            var app = await applicationManager.FindByClientIdAsync(clientId!);
            if (app is null || !await applicationManager.ValidateClientSecretAsync(app, clientSecret!))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Invalid client credentials."
                    }));
            }

            var phone = request["phone"]?.ToString();
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new { error = "invalid_phone" });
            }

            var normalizedPhone = new string(phone.Where(char.IsDigit).ToArray());
            var users = await userManager.Users
                .Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(normalizedPhone))
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound(new { error = "user_not_found" });
            }

            if (users.Count > 1)
            {
                return Conflict(new { error = "multiple_users_with_same_phone" });
            }

            var user = users[0];
            if (!user.Active)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is inactive."
                    }));
            }

            var phoneClaims = new List<Claim>
            {
                new(Claims.Subject, user.Id.ToString()),
                new(Claims.Email, user.Email ?? ""),
                new(Claims.Name, user.FullName),
                new("phone_number", user.PhoneNumber ?? normalizedPhone)
            };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
                phoneClaims.Add(new Claim(Claims.Role, role));

            var phoneIdentity = new ClaimsIdentity(phoneClaims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var phonePrincipal = new ClaimsPrincipal(phoneIdentity);

            phonePrincipal.SetScopes(request.GetScopes());
            phonePrincipal.SetResources("authforge-api");

            foreach (var claim in phonePrincipal.Claims)
                claim.SetDestinations(GetDestinations(claim, phonePrincipal));

            return SignIn(phonePrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal!;
        var user = await userManager.FindByIdAsync(principal.GetClaim(Claims.Subject)!);

        if (user is null)
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id.ToString(),
            [Claims.Name] = user.FullName,
            [Claims.Email] = user.Email!
        };

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            claims["phone_number"] = user.PhoneNumber;

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Count > 0)
            claims[Claims.Role] = roles.Count == 1 ? (object)roles[0] : roles;

        return Ok(claims);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile) || principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
