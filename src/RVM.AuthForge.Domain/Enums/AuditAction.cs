namespace RVM.AuthForge.Domain.Enums;

public enum AuditAction
{
    Login,
    LoginFailed,
    Logout,
    Register,
    EmailConfirmed,
    PasswordReset,
    PasswordChanged,
    Enable2FA,
    Disable2FA,
    RoleAssigned,
    RoleRemoved,
    ApiKeyCreated,
    ApiKeyRevoked,
    ClientCreated,
    ClientUpdated,
    AccountDeleted
}
