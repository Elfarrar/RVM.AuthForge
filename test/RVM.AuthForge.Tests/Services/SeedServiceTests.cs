using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenIddict.Abstractions;
using RVM.AuthForge.API.Services;
using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Infrastructure.Data;
using RVM.AuthForge.Tests.Helpers;

namespace RVM.AuthForge.Tests.Services;

public class SeedServiceTests
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static IServiceProvider BuildServiceProvider(
        Mock<UserManager<ApplicationUser>> userMgr,
        Mock<RoleManager<ApplicationRole>> roleMgr,
        Mock<IOpenIddictApplicationManager> appMgr,
        AuthForgeDbContext db)
    {
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(userMgr.Object);
        services.AddSingleton(roleMgr.Object);
        services.AddSingleton(appMgr.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = new Mock<IServiceScopeFactory>();
        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(provider);
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        var rootServices = new ServiceCollection();
        rootServices.AddSingleton(db);
        rootServices.AddSingleton(userMgr.Object);
        rootServices.AddSingleton(roleMgr.Object);
        rootServices.AddSingleton(appMgr.Object);
        rootServices.AddSingleton(scopeFactory.Object);

        return rootServices.BuildServiceProvider();
    }

    [Fact]
    public async Task StartAsync_SeedsRoles_WhenTheyDontExist()
    {
        using var db = TestDbContext.Create();
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();
        var appMgr = new Mock<IOpenIddictApplicationManager>();

        roleMgr.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleMgr.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>())).ReturnsAsync(IdentityResult.Success);

        // Admin user does not exist
        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // No oauth client
        appMgr.Setup(m => m.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);
        appMgr.Setup(m => m.CreateAsync(It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        var services = BuildServiceProvider(userMgr, roleMgr, appMgr, db);
        var config = BuildConfig(new Dictionary<string, string?> { ["Seed:AdminEmail"] = "admin@test.com" });

        var seedService = new SeedService(services, config);
        await seedService.StartAsync(CancellationToken.None);

        roleMgr.Verify(r => r.CreateAsync(It.Is<ApplicationRole>(role => role.Name == "Admin")), Times.Once);
        roleMgr.Verify(r => r.CreateAsync(It.Is<ApplicationRole>(role => role.Name == "User")), Times.Once);
    }

    [Fact]
    public async Task StartAsync_SkipsRoleCreation_WhenRolesAlreadyExist()
    {
        using var db = TestDbContext.Create();
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();
        var appMgr = new Mock<IOpenIddictApplicationManager>();

        roleMgr.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true); // Already exist
        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        appMgr.Setup(m => m.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);
        appMgr.Setup(m => m.CreateAsync(It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        var services = BuildServiceProvider(userMgr, roleMgr, appMgr, db);
        var config = BuildConfig([]);

        var seedService = new SeedService(services, config);
        await seedService.StartAsync(CancellationToken.None);

        roleMgr.Verify(r => r.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Fact]
    public async Task StartAsync_SkipsAdminCreation_WhenAdminAlreadyExists()
    {
        using var db = TestDbContext.Create();
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();
        var appMgr = new Mock<IOpenIddictApplicationManager>();

        roleMgr.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(IdentityMocks.MakeUser("admin@test.com", "Admin")); // Admin already exists
        appMgr.Setup(m => m.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);
        appMgr.Setup(m => m.CreateAsync(It.IsAny<OpenIddictApplicationDescriptor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        var services = BuildServiceProvider(userMgr, roleMgr, appMgr, db);
        var config = BuildConfig([]);

        var seedService = new SeedService(services, config);
        await seedService.StartAsync(CancellationToken.None);

        userMgr.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task StopAsync_CompletesSuccessfully()
    {
        using var db = TestDbContext.Create();
        var services = BuildServiceProvider(
            IdentityMocks.CreateUserManager(),
            IdentityMocks.CreateRoleManager(),
            new Mock<IOpenIddictApplicationManager>(),
            db);

        var config = BuildConfig([]);
        var seedService = new SeedService(services, config);

        // Should complete without throwing
        await seedService.StopAsync(CancellationToken.None);
    }
}
