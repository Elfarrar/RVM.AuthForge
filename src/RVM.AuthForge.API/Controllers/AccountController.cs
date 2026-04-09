using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Validation.AspNetCore;

namespace RVM.AuthForge.API.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAuditLogService audit) : ControllerBase
{
    // --- Registration & Login ---

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await userManager.AddToRoleAsync(user, "User");
        await audit.LogAsync(AuditAction.Register, user.Id.ToString(), user.Email, GetIp(), GetAgent());

        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.Active)
        {
            await audit.LogAsync(AuditAction.LoginFailed, request.Email, request.Email, GetIp(), GetAgent());
            return Unauthorized(new { error = "Invalid credentials." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return Unauthorized(new { error = "Account is locked. Try again later." });
        if (result.RequiresTwoFactor)
            return Ok(new { requiresTwoFactor = true, userId = user.Id });
        if (!result.Succeeded)
        {
            await audit.LogAsync(AuditAction.LoginFailed, user.Id.ToString(), user.Email, GetIp(), GetAgent());
            return Unauthorized(new { error = "Invalid credentials." });
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        await audit.LogAsync(AuditAction.Login, user.Id.ToString(), user.Email, GetIp(), GetAgent());

        return Ok(new { message = "Login successful.", userId = user.Id, fullName = user.FullName });
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        await signInManager.SignOutAsync();
        if (userId is not null)
            await audit.LogAsync(AuditAction.Logout, userId, ipAddress: GetIp());
        return Ok(new { message = "Logged out." });
    }

    // --- Profile ---

    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        return Ok(new
        {
            user.Id, user.FullName, user.Email, user.AvatarUrl,
            user.EmailConfirmed, user.TwoFactorEnabled, user.CreatedAt
        });
    }

    [HttpPut("profile")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        user.FullName = request.FullName;
        user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);
        return Ok(new { message = "Profile updated." });
    }

    // --- Password ---

    [HttpPost("change-password")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await audit.LogAsync(AuditAction.PasswordChanged, user.Id.ToString(), user.Email, GetIp(), GetAgent());
        return Ok(new { message = "Password changed." });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            // In production, send email with token. For demo, return it.
            return Ok(new { message = "If the email exists, a reset link has been sent.", token });
        }
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return BadRequest(new { error = "Invalid request." });

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await audit.LogAsync(AuditAction.PasswordReset, user.Id.ToString(), user.Email, GetIp(), GetAgent());
        return Ok(new { message = "Password reset successful." });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null) return BadRequest(new { error = "Invalid request." });

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await audit.LogAsync(AuditAction.EmailConfirmed, user.Id.ToString(), user.Email, GetIp(), GetAgent());
        return Ok(new { message = "Email confirmed." });
    }

    // --- Two-Factor Authentication ---

    [HttpGet("2fa/status")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> TwoFactorStatus()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();
        return Ok(new { enabled = user.TwoFactorEnabled });
    }

    [HttpPost("2fa/enable")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var uri = GenerateQrCodeUri(user.Email!, key!);
        return Ok(new { sharedKey = key, authenticatorUri = uri });
    }

    [HttpPost("2fa/verify")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorVerifyRequest request)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

        if (!valid) return BadRequest(new { error = "Invalid verification code." });

        await userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        await audit.LogAsync(AuditAction.Enable2FA, user.Id.ToString(), user.Email, GetIp(), GetAgent());
        return Ok(new { recoveryCodes });
    }

    [HttpPost("2fa/disable")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await audit.LogAsync(AuditAction.Disable2FA, user.Id.ToString(), user.Email, GetIp(), GetAgent());

        return Ok(new { message = "2FA disabled." });
    }

    [HttpPost("2fa/recovery-codes")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return Ok(new { recoveryCodes = codes });
    }

    // --- Helpers ---

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return userId is null ? null : await userManager.FindByIdAsync(userId);
    }

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? GetAgent() => Request.Headers.UserAgent.ToString();

    private static string GenerateQrCodeUri(string email, string key)
    {
        const string issuer = "RVM.AuthForge";
        return $"otpauth://totp/{UrlEncoder.Default.Encode(issuer)}:{UrlEncoder.Default.Encode(email)}" +
               $"?secret={key}&issuer={UrlEncoder.Default.Encode(issuer)}&digits=6";
    }
}

// --- Request DTOs ---

public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record UpdateProfileRequest(string FullName, string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record ConfirmEmailRequest(string UserId, string Token);
public record TwoFactorVerifyRequest(string Code);
