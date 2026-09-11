using WareStockApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace WareStockApi.Web.Endpoints;

/// <summary>
/// Hand-rolled (not MediatR) authentication endpoints, following the same pattern as the
/// template's original <c>Logout</c> handler: <see cref="UserManager{TUser}"/> is injected
/// directly. Kept separate from <see cref="Users"/> (user CRUD, which requires auth). Only
/// login/refresh are exposed — register/forgot-password/otp-verify have no wired UI in the
/// frontend (see docs/api-spec/openapi.yaml's scope note) and were dropped accordingly.
/// </summary>
public class Auth : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login").AllowAnonymous();
        groupBuilder.MapPost(Refresh, "refresh").AllowAnonymous();
    }

    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);

    public record AuthUser(string AccountNo, string Email, IReadOnlyList<string> Role, long Exp, string? Name, string? Avatar);
    public record AuthLoginResponse(string AccessToken, string RefreshToken, AuthUser User);

    [EndpointSummary("Log in")]
    [EndpointDescription("Authenticates a user by email and password and returns a JWT access token plus a refresh token.")]
    public static async Task<Results<Ok<ApiResponse<AuthLoginResponse>>, JsonHttpResult<ApiErrorResponse>>> Login(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService tokenService,
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var passwordValid = user is not null && await userManager.CheckPasswordAsync(user, request.Password);

        if (user is null || !passwordValid)
        {
            return TypedResults.Json(UnauthorizedError("Invalid email or password."), statusCode: StatusCodes.Status401Unauthorized);
        }

        var tokens = await tokenService.IssueTokensAsync(user, cancellationToken);

        return TypedResults.Ok(ToResponse(tokens).ToApiResponse("Login successful."));
    }

    [EndpointSummary("Refresh access token")]
    [EndpointDescription("Exchanges a valid, unexpired refresh token for a new access token and refresh token. The redeemed refresh token is revoked (rotation) — it cannot be used again.")]
    public static async Task<Results<Ok<ApiResponse<AuthLoginResponse>>, JsonHttpResult<ApiErrorResponse>>> Refresh(
        IJwtTokenService tokenService,
        RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var tokens = await tokenService.RefreshAsync(request.RefreshToken, cancellationToken);

        if (tokens is null)
        {
            return TypedResults.Json(UnauthorizedError("Invalid or expired refresh token."), statusCode: StatusCodes.Status401Unauthorized);
        }

        return TypedResults.Ok(ToResponse(tokens).ToApiResponse("Token refreshed."));
    }

    private static AuthLoginResponse ToResponse(AuthTokens tokens) => new(
        tokens.AccessToken,
        tokens.RefreshToken,
        new AuthUser(
            AccountNo: tokens.User.Id,
            Email: tokens.User.Email ?? string.Empty,
            Role: tokens.Roles,
            Exp: tokens.AccessTokenExpiresUnix,
            Name: tokens.User.DisplayName,
            Avatar: tokens.User.AvatarUrl));

    private static ApiErrorResponse UnauthorizedError(string message) => new()
    {
        ErrorCode = "UNAUTHORIZED",
        Message = message,
        RequestId = Guid.NewGuid().ToString(),
        StatusCode = StatusCodes.Status401Unauthorized
    };
}
