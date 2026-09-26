using Microsoft.AspNetCore.Identity;
using Prodora.WebUI.Identity;

sealed class MemoryUserStore : IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserSecurityStampStore<ApplicationUser>
{
    private readonly Dictionary<string, ApplicationUser> users = new();
    public void Dispose() { }
    public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) { users[user.Id] = user; return Task.FromResult(IdentityResult.Success); }
    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) { users[user.Id] = user; return Task.FromResult(IdentityResult.Success); }
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) { users.Remove(user.Id); return Task.FromResult(IdentityResult.Success); }
    public Task<ApplicationUser?> FindByIdAsync(string id, CancellationToken ct) => Task.FromResult(users.GetValueOrDefault(id));
    public Task<ApplicationUser?> FindByNameAsync(string name, CancellationToken ct) => Task.FromResult(users.Values.FirstOrDefault(u => u.NormalizedUserName == name));
    public Task<string> GetUserIdAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.Id);
    public Task<string?> GetUserNameAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.UserName);
    public Task SetUserNameAsync(ApplicationUser u, string? value, CancellationToken ct) { u.UserName = value; return Task.CompletedTask; }
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.NormalizedUserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser u, string? value, CancellationToken ct) { u.NormalizedUserName = value; return Task.CompletedTask; }
    public Task SetPasswordHashAsync(ApplicationUser u, string? value, CancellationToken ct) { u.PasswordHash = value; return Task.CompletedTask; }
    public Task<string?> GetPasswordHashAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.PasswordHash);
    public Task<bool> HasPasswordAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.PasswordHash != null);
    public Task SetEmailAsync(ApplicationUser u, string? value, CancellationToken ct) { u.Email = value; return Task.CompletedTask; }
    public Task<string?> GetEmailAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.Email);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.EmailConfirmed);
    public Task SetEmailConfirmedAsync(ApplicationUser u, bool value, CancellationToken ct) { u.EmailConfirmed = value; return Task.CompletedTask; }
    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken ct) => Task.FromResult(users.Values.FirstOrDefault(u => u.NormalizedEmail == email));
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.NormalizedEmail);
    public Task SetNormalizedEmailAsync(ApplicationUser u, string? value, CancellationToken ct) { u.NormalizedEmail = value; return Task.CompletedTask; }
    public Task SetSecurityStampAsync(ApplicationUser u, string value, CancellationToken ct) { u.SecurityStamp = value; return Task.CompletedTask; }
    public Task<string?> GetSecurityStampAsync(ApplicationUser u, CancellationToken ct) => Task.FromResult(u.SecurityStamp);
}
