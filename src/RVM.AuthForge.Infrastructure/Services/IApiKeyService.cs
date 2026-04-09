using RVM.AuthForge.Domain.Entities;

namespace RVM.AuthForge.Infrastructure.Services;

public interface IApiKeyService
{
    Task<(ApplicationApiKey Key, string PlainTextKey)> CreateAsync(string appId, string name);
    Task<ApplicationApiKey?> ValidateAsync(string plainTextKey);
    Task RevokeAsync(Guid id);
    Task<List<ApplicationApiKey>> ListAsync(string? appId = null);
}
