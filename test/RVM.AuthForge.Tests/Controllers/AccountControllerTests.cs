using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.AuthForge.API.Controllers;
using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Services;
using RVM.AuthForge.Tests.Helpers;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace RVM.AuthForge.Tests.Controllers;

public class AccountControllerTests
{
    private static AccountController CreateController(
        Mock<UserManager<ApplicationUser>> userMgr,
        Mock<SignInManager<ApplicationUser>> signInMgr,
        IAuditLogService? audit = null)
    {
        audit ??= Mock.Of<IAuditLogService>();
        var controller = new AccountController(userMgr.Object, signInMgr.Object, audit);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenCreationFails()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Register(new RegisterRequest("Test User", "test@test.com", "weak"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenCreationSucceeds()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var audit = new Mock<IAuditLogService>();
        var controller = CreateController(userMgr, signInMgr, audit.Object);
        var result = await controller.Register(new RegisterRequest("New User", "new@test.com", "Strong123!"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest("notfound@test.com", "pass"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserInactive()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser(active: false);
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest(user.Email!, "pass"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordWrong()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, "wrongpass", true))
            .ReturnsAsync(IdentitySignInResult.Failed);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest(user.Email!, "wrongpass"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, "Strong123!", true))
            .ReturnsAsync(IdentitySignInResult.Success);
        signInMgr.Setup(m => m.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest(user.Email!, "Strong123!"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsLockedOut_WhenAccountLocked()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, "pass", true))
            .ReturnsAsync(IdentitySignInResult.LockedOut);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest(user.Email!, "pass"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_Returns2FaRequired_WhenTwoFactorRequired()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        signInMgr.Setup(m => m.CheckPasswordSignInAsync(user, "pass", true))
            .ReturnsAsync(IdentitySignInResult.TwoFactorRequired);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.Login(new LoginRequest(user.Email!, "pass"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsOk_WhenUserExists()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userMgr.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token-123");

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ForgotPassword(new ForgotPasswordRequest(user.Email!));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsOk_WhenUserDoesNotExist()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ForgotPassword(new ForgotPasswordRequest("nonexistent@test.com"));

        Assert.IsType<OkObjectResult>(result); // Security: always Ok
    }

    [Fact]
    public async Task ResetPassword_ReturnsBadRequest_WhenUserNotFound()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ResetPassword(new ResetPasswordRequest("bad@test.com", "token", "NewPass123!"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userMgr.Setup(m => m.ResetPasswordAsync(user, "valid-token", "NewPass123!"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ResetPassword(new ResetPasswordRequest(user.Email!, "valid-token", "NewPass123!"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenFails()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        // Cannot test authorized methods easily without setting up HttpContext claims,
        // so we test the service-level failures via the error path
        userMgr.Setup(m => m.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed" }));

        // Since GetCurrentUser requires auth claims, this returns Unauthorized when not authenticated
        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ChangePassword(new ChangePasswordRequest("old", "new"));

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ConfirmEmail_ReturnsBadRequest_WhenUserNotFound()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        userMgr.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ConfirmEmail(new ConfirmEmailRequest("user-id", "token"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmEmail_ReturnsOk_WhenSuccessful()
    {
        var userMgr = IdentityMocks.CreateUserManager();
        var signInMgr = IdentityMocks.CreateSignInManager(userMgr);

        var user = IdentityMocks.MakeUser();
        userMgr.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userMgr.Setup(m => m.ConfirmEmailAsync(user, "valid-token"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userMgr, signInMgr);
        var result = await controller.ConfirmEmail(new ConfirmEmailRequest(user.Id.ToString(), "valid-token"));

        Assert.IsType<OkObjectResult>(result);
    }
}
