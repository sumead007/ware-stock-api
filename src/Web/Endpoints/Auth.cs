using System.Text.Json;
using WareStockApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace WareStockApi.Web.Endpoints;

/// <summary>
/// Hand-rolled (not MediatR) authentication endpoint, following the same pattern as the
/// <c>Logout</c> handler: <see cref="SignInManager{TUser}"/>/<see cref="UserManager{TUser}"/> are
/// injected directly. Kept separate from <see cref="Users"/> (user CRUD, which requires auth).
/// Only <c>login</c> is exposed — register/forgot-password/otp-verify have no wired UI in the
/// frontend (see docs/api-spec/openapi.yaml's scope note) and were dropped accordingly.
/// </summary>
public class Auth : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login").AllowAnonymous();
    }

    public record LoginRequest(string Email, string Password);

    public record AuthUser(string AccountNo, string Email, IReadOnlyList<string> Role, long Exp, string? Name, string? Avatar);
    public record AuthLoginResponse(string AccessToken, AuthUser User);

    [EndpointSummary("Log in")]
    [EndpointDescription("Authenticates a user by email and password and returns a bearer access token.")]
    public static async Task<Results<Ok<ApiResponse<AuthLoginResponse>>, JsonHttpResult<ApiErrorResponse>>> Login(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        var passwordValid = user is not null && await userManager.CheckPasswordAsync(user, request.Password);

        if (user is null || !passwordValid)
        {
            return TypedResults.Json(
                UnauthorizedError("Invalid email or password."),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = await BuildLoginResponseAsync(httpContext, userManager, signInManager, user);

        return TypedResults.Ok(response.ToApiResponse("Login successful."));
    }

    /// <summary>
    /// Signs the user in on the ASP.NET Identity bearer scheme and captures the framework-issued
    /// <c>{ accessToken, expiresIn }</c> payload by temporarily redirecting the response body to a
    /// buffer (the bearer token handler writes its own JSON response as part of <c>SignInAsync</c>),
    /// then composes our own <see cref="AuthLoginResponse"/> envelope from it.
    /// </summary>
    private static async Task<AuthLoginResponse> BuildLoginResponseAsync(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationUser user)
    {
        var originalBody = httpContext.Response.Body;
        await using var buffer = new MemoryStream();
        httpContext.Response.Body = buffer;

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignInAsync(user, isPersistent: false);

        httpContext.Response.Body = originalBody;
        buffer.Seek(0, SeekOrigin.Begin);

        var tokenPayload = await JsonSerializer.DeserializeAsync<BearerTokenPayload>(
            buffer, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var roles = await userManager.GetRolesAsync(user);
        var exp = DateTimeOffset.UtcNow.AddSeconds(tokenPayload?.ExpiresIn ?? 0).ToUnixTimeSeconds();

        var authUser = new AuthUser(
            AccountNo: user.Id,
            Email: user.Email ?? string.Empty,
            Role: roles.ToList(),
            Exp: exp,
            Name: user.DisplayName,
            Avatar: user.AvatarUrl);

        return new AuthLoginResponse(tokenPayload?.AccessToken ?? string.Empty, authUser);
    }

    private static ApiErrorResponse UnauthorizedError(string message) => new()
    {
        ErrorCode = "UNAUTHORIZED",
        Message = message,
        RequestId = Guid.NewGuid().ToString(),
        StatusCode = StatusCodes.Status401Unauthorized
    };

    private record BearerTokenPayload(string AccessToken, int ExpiresIn);
}
