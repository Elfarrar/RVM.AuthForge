using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace RVM.AuthForge.API.Services;

public class SeedService(IServiceProvider services, IConfiguration config) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthForgeDbContext>();
        await db.Database.EnsureCreatedAsync(ct);

        await SeedRolesAsync(scope);
        await SeedAdminUserAsync(scope);
        await SeedOAuthClientsAsync(scope);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private static async Task SeedRolesAsync(IServiceScope scope)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole { Name = role, Description = $"{role} role" });
        }
    }

    private async Task SeedAdminUserAsync(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = config["Seed:AdminEmail"] ?? config["Admin:Email"] ?? "admin@authforge.dev";
        var adminPassword = config["Seed:AdminPassword"] ?? config["Admin:Password"] ?? "Admin123!";
        var adminFullName = config["Seed:AdminFullName"] ?? "System Administrator";

        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = adminFullName,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, adminPassword);
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    private async Task SeedOAuthClientsAsync(IServiceScope scope)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("authforge-portal") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "authforge-portal",
                DisplayName = "RVM.AuthForge Portal (React SPA)",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                RedirectUris = { new Uri("http://localhost:5173/callback") },
                PostLogoutRedirectUris = { new Uri("http://localhost:5173/") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }

        // Internal client for service-to-service auth (ERP, Gypsy, etc.)
        var internalSecret = config["Seed:InternalClientSecret"];
        if (!string.IsNullOrWhiteSpace(internalSecret) && await manager.FindByClientIdAsync("rvm-internal") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "rvm-internal",
                ClientSecret = internalSecret,
                DisplayName = "RVM Internal (Service-to-Service)",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.GrantType + "phone_login",
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                    OpenIddictConstants.Permissions.Prefixes.Scope + "phone"
                }
            });
        }

        // MiniERP client
        var miniErpSecret = config["Seed:MiniErpClientSecret"];
        if (!string.IsNullOrWhiteSpace(miniErpSecret) && await manager.FindByClientIdAsync("minierp") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "minierp",
                ClientSecret = miniErpSecret,
                DisplayName = "RVM.ERP (MiniERP)",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                RedirectUris = { new Uri("https://erp.rvmtech.com.br/signin-oidc") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api"
                }
            });
        }
    }
}
