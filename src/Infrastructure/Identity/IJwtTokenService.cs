namespace WareStockApi.Infrastructure.Identity;

/// <summary>
/// Mints JWT access tokens and manages the DB-backed refresh token that accompanies each one.
/// Lives in Infrastructure (not behind an Application-layer interface) because it depends on
/// <see cref="ApplicationUser"/> directly — the same reason <c>Auth.cs</c> injects
/// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/> straight from Web.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Issues a fresh access + refresh token pair for an already-authenticated user (login).</summary>
    Task<AuthTokens> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken);

    /// <summary>
    /// Redeems a refresh token for a new access + refresh token pair, revoking the redeemed one
    /// (rotation). Returns <c>null</c> if the token is unknown, expired, or already revoked.
    /// </summary>
    Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
}

public record AuthTokens(
    string AccessToken,
    long AccessTokenExpiresUnix,
    string RefreshToken,
    ApplicationUser User,
    IReadOnlyList<string> Roles);
