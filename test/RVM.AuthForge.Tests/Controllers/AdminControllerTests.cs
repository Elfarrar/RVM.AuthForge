using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OpenIddict.Abstractions;
using RVM.AuthForge.API.Controllers;
using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Services;
using RVM.AuthForge.Tests.Helpers;

namespace RVM.AuthForge.Tests.Controllers;

public class AdminControllerTests
{
    private static AdminController CreateController(
        Mock<UserManager<ApplicationUser>> userMgr,
        Mock<RoleManager<ApplicationRole>>? roleMgr = null,
        IApiKeyService? apiKeyService = null,
        IAuditLogService? auditLog = null,
        IOpenIddictApplicationManager? clientManager = null)
    {
        roleMgr ??= IdentityMocks.CreateRoleManager();
        apiKeyService ??= Mock.Of<IApiKeyService>();
        auditLog ??= Mock.Of<IAuditLogService>();
        clientManager ??= Mock.Of<IOpenIddictApplicationManager>();

        var controller = new AdminController(userMgr.Object, roleMgr.Object, apiKeyService, auditLog, clientManager);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // --- User Tests ---

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.GetUser(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetUser_ReturnsOk_WhenUserExists()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var user = IdentityMocks.MakeUser();

        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["User"]);

        var controller = CreateController(userMgr);
        var result = await controller.GetUser(user.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFound_WhenUserMissing()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.UpdateUser(Guid.NewGuid(), new AdminUpdateUserRequest("New Name", true));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var user = IdentityMocks.MakeUser();

        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr);
        var result = await controller.UpdateUser(user.Id, new AdminUpdateUserRequest("Updated Name", true));

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Updated Name", user.FullName);
    }

    [Fact]
    public async Task AssignRole_ReturnsNotFound_WhenUserMissing()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.AssignRole(Guid.NewGuid(), new RoleAssignRequest("Admin"));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignRole_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var auditLog = new Mock<IAuditLogService>();
        var user = IdentityMocks.MakeUser();

        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.AddToRoleAsync(user, "Admin")).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr, auditLog: auditLog.Object);
        var result = await controller.AssignRole(user.Id, new RoleAssignRequest("Admin"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task LockUser_ReturnsNotFound_WhenUserMissing()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.LockUser(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task LockUser_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var user = IdentityMocks.MakeUser();

        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr);
        var result = await controller.LockUser(user.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UnlockUser_ReturnsNotFound_WhenUserMissing()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr);
        var result = await controller.UnlockUser(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UnlockUser_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var user = IdentityMocks.MakeUser();

        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.SetLockoutEndDateAsync(user, null)).ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(m => m.ResetAccessFailedCountAsync(user)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr);
        var result = await controller.UnlockUser(user.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    // --- Role Tests ---

    [Fact]
    public async Task CreateRole_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();

        roleMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr, roleMgr);
        var result = await controller.CreateRole(new CreateRoleRequest("Moderator", "Moderator role"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WhenFails()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();

        roleMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role already exists" }));

        var controller = CreateController(userMgr, roleMgr);
        var result = await controller.CreateRole(new CreateRoleRequest("Admin", null));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleMissing()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var roleMgr = IdentityMocks.CreateRoleManager();

        roleMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationRole?)null);

        var controller = CreateController(userMgr, roleMgr);
        var result = await controller.DeleteRole(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    // --- API Key Tests ---

    [Fact]
    public async Task CreateApiKey_ReturnsOk_WithPlainKey()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var apiKeyService = new Mock<IApiKeyService>();
        var auditLog = new Mock<IAuditLogService>();

        var fakeKey = new ApplicationApiKey { AppId = "my-app", Name = "Test", KeyHash = "hash", KeyPrefix = "ABCDEFGH" };
        apiKeyService.Setup(s => s.CreateAsync("my-app", "Test")).ReturnsAsync((fakeKey, "ABCDEFGH_plaintext"));

        var controller = CreateController(userMgr, apiKeyService: apiKeyService.Object, auditLog: auditLog.Object);
        var result = await controller.CreateApiKey(new CreateApiKeyRequest("my-app", "Test"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RevokeApiKey_ReturnsOk()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var apiKeyService = new Mock<IApiKeyService>();
        var auditLog = new Mock<IAuditLogService>();
        var id = Guid.NewGuid();

        apiKeyService.Setup(s => s.RevokeAsync(id)).Returns(Task.CompletedTask);

        var controller = CreateController(userMgr, apiKeyService: apiKeyService.Object, auditLog: auditLog.Object);
        var result = await controller.RevokeApiKey(id);

        Assert.IsType<OkObjectResult>(result);
    }

    // --- Audit Log Tests ---

    [Fact]
    public async Task GetAuditLog_ReturnsOk()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var auditLog = new Mock<IAuditLogService>();

        auditLog.Setup(s => s.GetEntriesAsync(null, null, null, null, 1, 50)).ReturnsAsync([]);
        auditLog.Setup(s => s.CountAsync(null, null, null, null)).ReturnsAsync(0);

        var controller = CreateController(userMgr, auditLog: auditLog.Object);
        var result = await controller.GetAuditLog(null, null, null, null, 1, 50);

        Assert.IsType<OkObjectResult>(result);
    }
}
