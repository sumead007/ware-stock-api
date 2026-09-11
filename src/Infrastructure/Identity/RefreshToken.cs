namespace WareStockApi.Infrastructure.Identity;

/// <summary>
/// A single-use refresh token issued alongside a JWT access token. Only the SHA-256 hash of the
/// token value is persisted — the raw value is handed to the client once and never stored.
/// Rotation: redeeming a token via <see cref="IJwtTokenService.RefreshAsync"/> immediately revokes
/// it and issues a new one, so a captured token can be used at most once before it stops working.
/// </summary>
public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
