using RVM.AuthForge.Domain.Entities;
using RVM.AuthForge.Domain.Enums;
using RVM.AuthForge.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace RVM.AuthForge.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IApiKeyService apiKeyService,
    IAuditLogService auditLog,
    IOpenIddictApplicationManager clientManager) : ControllerBase
{
    // --- Users ---

    [HttpGet("users")]
    public async Task<IActionResult> ListUsers([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = userManager.Users.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Email!.Contains(search) || u.FullName.Contains(search));

        var total = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.FullName, u.Email, u.Active, u.EmailConfirmed, u.TwoFactorEnabled, u.CreatedAt })
            .ToListAsync();

        return Ok(new { total, page, pageSize, users });
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id, user.FullName, user.Email, user.Active,
            user.EmailConfirmed, user.TwoFactorEnabled, user.CreatedAt,
            user.LockoutEnd, user.AccessFailedCount,
            roles
        });
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        user.FullName = request.FullName;
        user.Active = request.Active;
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);
        return Ok(new { message = "User updated." });
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] RoleAssignRequest request)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        var result = await userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await auditLog.LogAsync(AuditAction.RoleAssigned, user.Id.ToString(), user.Email,
            details: $"Role: {request.Role}");
        return Ok(new { message = $"Role '{request.Role}' assigned." });
    }

    [HttpDelete("users/{id:guid}/roles/{role}")]
    public async Task<IActionResult> RemoveRole(Guid id, string role)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        await userManager.RemoveFromRoleAsync(user, role);
        await auditLog.LogAsync(AuditAction.RoleRemoved, user.Id.ToString(), user.Email,
            details: $"Role: {role}");
        return Ok(new { message = $"Role '{role}' removed." });
    }

    [HttpPost("users/{id:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        return Ok(new { message = "User locked." });
    }

    [HttpPost("users/{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return NotFound();

        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
        return Ok(new { message = "User unlocked." });
    }

    // --- Roles ---

    [HttpGet("roles")]
    public async Task<IActionResult> ListRoles()
    {
        var roles = await roleManager.Roles
            .Select(r => new { r.Id, r.Name, r.Description, r.CreatedAt })
            .ToListAsync();
        return Ok(roles);
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = request.Name,
            Description = request.Description
        });
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        return Ok(new { message = $"Role '{request.Name}' created." });
    }

    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await roleManager.FindByIdAsync(id.ToString());
        if (role is null) return NotFound();

        await roleManager.DeleteAsync(role);
        return Ok(new { message = "Role deleted." });
    }

    // --- API Keys ---

    [HttpGet("api-keys")]
    public async Task<IActionResult> ListApiKeys([FromQuery] string? appId)
    {
        var keys = await apiKeyService.ListAsync(appId);
        return Ok(keys.Select(k => new
        {
            k.Id, k.AppId, k.Name, k.KeyPrefix, k.Active, k.CreatedAt, k.RevokedAt
        }));
    }

    [HttpPost("api-keys")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        var (key, plainKey) = await apiKeyService.CreateAsync(request.AppId, request.Name);
        await auditLog.LogAsync(AuditAction.ApiKeyCreated, "admin", details: $"Key: {key.KeyPrefix}...");
        return Ok(new { key.Id, key.AppId, key.Name, key.KeyPrefix, apiKey = plainKey });
    }

    [HttpPost("api-keys/{id:guid}/revoke")]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        await apiKeyService.RevokeAsync(id);
        await auditLog.LogAsync(AuditAction.ApiKeyRevoked, "admin", details: $"KeyId: {id}");
        return Ok(new { message = "API key revoked." });
    }

    // --- OAuth Clients ---

    [HttpGet("clients")]
    public async Task<IActionResult> ListClients()
    {
        var clients = new List<object>();
        await foreach (var client in clientManager.ListAsync())
        {
            var id = await clientManager.GetIdAsync(client);
            var clientId = await clientManager.GetClientIdAsync(client);
            var displayName = await clientManager.GetDisplayNameAsync(client);
            var clientType = await clientManager.GetClientTypeAsync(client);
            clients.Add(new { id, clientId, displayName, clientType });
        }
        return Ok(clients);
    }

    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = request.ClientId,
            DisplayName = request.DisplayName,
            ClientType = request.IsConfidential
                ? OpenIddictConstants.ClientTypes.Confidential
                : OpenIddictConstants.ClientTypes.Public,
            ClientSecret = request.ClientSecret
        };

        foreach (var uri in request.RedirectUris ?? [])
            descriptor.RedirectUris.Add(new Uri(uri));

        foreach (var perm in request.Permissions ?? [])
            descriptor.Permissions.Add(perm);

        await clientManager.CreateAsync(descriptor);
        await auditLog.LogAsync(AuditAction.ClientCreated, "admin", details: $"ClientId: {request.ClientId}");
        return Ok(new { message = $"Client '{request.ClientId}' created." });
    }

    [HttpPut("clients/{id}")]
    public async Task<IActionResult> UpdateClient(string id, [FromBody] UpdateClientRequest request)
    {
        var client = await clientManager.FindByIdAsync(id);
        if (client is null) return NotFound();

        var descriptor = new OpenIddictApplicationDescriptor();
        await clientManager.PopulateAsync(descriptor, client);

        descriptor.DisplayName = request.DisplayName;
        descriptor.RedirectUris.Clear();
        foreach (var uri in request.RedirectUris ?? [])
            descriptor.RedirectUris.Add(new Uri(uri));

        await clientManager.UpdateAsync(client, descriptor);
        await auditLog.LogAsync(AuditAction.ClientUpdated, "admin", details: $"ClientId: {id}");
        return Ok(new { message = "Client updated." });
    }

    // --- Audit Log ---

    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] AuditAction? action,
        [FromQuery] string? userId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var entries = await auditLog.GetEntriesAsync(action, userId, from, to, page, pageSize);
        var total = await auditLog.CountAsync(action, userId, from, to);
        return Ok(new { total, page, pageSize, entries });
    }
}

// --- Request DTOs ---

public record AdminUpdateUserRequest(string FullName, bool Active);
public record RoleAssignRequest(string Role);
public record CreateRoleRequest(string Name, string? Description);
public record CreateApiKeyRequest(string AppId, string Name);
public record CreateClientRequest(string ClientId, string DisplayName, bool IsConfidential, string? ClientSecret, string[]? RedirectUris, string[]? Permissions);
public record UpdateClientRequest(string DisplayName, string[]? RedirectUris);
