using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WareStockApi.Domain.Enums;
using WareStockApi.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WareStockApi.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    /// <summary>
    /// Claim type used for role claims in minted tokens. "role" isn't one of the RFC 7519
    /// registered claim names, but it's the conventional short name (vs. the long
    /// System.Security.Claims.ClaimTypes.Role URI) — must match TokenValidationParameters.RoleClaimType
    /// in DependencyInjection.cs and the lookup in Web/Services/CurrentUser.cs.
    /// </summary>
    public const string RoleClaimType = "role";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _options;

    public JwtTokenService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, JwtOptions options)
    {
        _context = context;
        _userManager = userManager;
        _options = options;
    }

    public async Task<AuthTokens> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expiresUnix) = GenerateAccessToken(user, roles);
        var refreshToken = CreateRefreshToken(user.Id);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, expiresUnix, refreshToken, user, roles.ToList());
    }

    public async Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = Hash(refreshToken);

        var existing = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            return null;
        }

        if (existing.User.Status != UserStatus.Active)
        {
            existing.RevokedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return null;
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;

        var roles = await _userManager.GetRolesAsync(existing.User);
        var (accessToken, expiresUnix) = GenerateAccessToken(existing.User, roles);
        var newRefreshToken = CreateRefreshToken(existing.UserId);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, expiresUnix, newRefreshToken, existing.User, roles.ToList());
    }

    private (string Token, long ExpiresUnix) GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(RoleClaimType, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires.ToUnixTimeSeconds());
    }

    private string CreateRefreshToken(string userId)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = Hash(rawToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays)
        });

        return rawToken;
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
