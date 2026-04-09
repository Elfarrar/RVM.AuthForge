using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Infrastructure.Data;
using RVM.AuthForge.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RVM.AuthForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthForgeInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthForgeDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Default"));
            options.UseOpenIddict();
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AuthForgeDbContext>()
        .AddDefaultTokenProviders();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AuthForgeDbContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize");
                options.SetTokenEndpointUris("connect/token");
                options.SetUserInfoEndpointUris("connect/userinfo");
                options.SetEndSessionEndpointUris("connect/logout");

                options.AllowAuthorizationCodeFlow();
                options.AllowClientCredentialsFlow();
                options.AllowRefreshTokenFlow();
                options.RequireProofKeyForCodeExchange();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                options.RegisterScopes("openid", "profile", "email", "api");

                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();

                options.DisableAccessTokenEncryption();

                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableStatusCodePagesIntegration();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        return services;
    }
}
