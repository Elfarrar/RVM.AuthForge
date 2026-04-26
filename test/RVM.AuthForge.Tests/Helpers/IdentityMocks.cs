using Microsoft.AspNetCore.Identity;
using Moq;
using RVM.AuthForge.Domain.Entities;

namespace RVM.AuthForge.Tests.Helpers;

public static class IdentityMocks
{
    public static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
        mgr.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());
        return mgr;
    }

    public static Mock<SignInManager<ApplicationUser>> CreateSignInManager(Mock<UserManager<ApplicationUser>> userManager)
    {
        var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var mgr = new Mock<SignInManager<ApplicationUser>>(
            userManager.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            null!, null!, null!, null!);
        return mgr;
    }

    public static Mock<RoleManager<ApplicationRole>> CreateRoleManager()
    {
        var store = new Mock<IRoleStore<ApplicationRole>>();
        var mgr = new Mock<RoleManager<ApplicationRole>>(
            store.Object, null!, null!, null!, null!);
        return mgr;
    }

    public static ApplicationUser MakeUser(string email = "test@test.com", string fullName = "Test User", bool active = true)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName,
            Active = active,
            EmailConfirmed = true
        };
    }
}
