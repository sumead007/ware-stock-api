namespace WareStockApi.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key (HMAC-SHA256, needs 32+ bytes). The value in appsettings.json is a
    /// development-only placeholder — override via user-secrets or environment/Key Vault for any
    /// real deployment, the same way <c>ConnectionStrings:WareStockApiDb</c> is expected to be.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 60;

    public int RefreshTokenDays { get; set; } = 7;
}
