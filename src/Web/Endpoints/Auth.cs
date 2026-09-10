using System.Text.Json;
using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace WareStockApi.Web.Endpoints;

/// <summary>
/// Hand-rolled (not MediatR) authentication endpoints, following the same pattern as the
/// <c>Logout</c> handler: <see cref="SignInManager{TUser}"/>/<see cref="UserManager{TUser}"/> are
/// injected directly. Kept separate from <see cref="Users"/> (user CRUD, which requires auth).
/// </summary>
public class Auth : IEndpointGroup
{
    public static string? RoutePrefix => "/v1/auth";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Login, "login").AllowAnonymous();
        groupBuilder.MapPost(Register, "register").AllowAnonymous();
        groupBuilder.MapPost(ForgotPassword, "forgot-password").AllowAnonymous();
        groupBuilder.MapPost(VerifyOtp, "otp/verify").AllowAnonymous();
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password);
    public record ForgotPasswordRequest(string Email);
    public record OtpVerifyRequest(string Email, string Otp);

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
                UnauthorizedError(httpContext, "Invalid email or password."),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = await BuildLoginResponseAsync(httpContext, userManager, signInManager, user);

        return TypedResults.Ok(response.ToApiResponse("Login successful."));
    }

    [EndpointSummary("Register")]
    [EndpointDescription("Creates a new user account and logs them in, returning a bearer access token.")]
    public static async Task<Created<ApiResponse<AuthLoginResponse>>> Register(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RegisterRequest request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new ConflictException("This email address is already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.Email.Split('@')[0],
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure(nameof(request.Password), e.Description)));
        }

        var response = await BuildLoginResponseAsync(httpContext, userManager, signInManager, user);

        return TypedResults.Created((string?)null, response.ToApiResponse("Registration successful.", StatusCodes.Status201Created));
    }

    [EndpointSummary("Forgot password")]
    [EndpointDescription("Generates an OTP for password reset. Always returns 200 whether or not the email exists, to avoid leaking account information.")]
    public static async Task<Ok<ApiResponse<object?>>> ForgotPassword(
        UserManager<ApplicationUser> userManager,
        ILogger<Auth> logger,
        ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            var otp = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "reset-password");
            logger.LogInformation("Password reset OTP for {Email}: {Otp}", request.Email, otp);
        }

        return TypedResults.Ok(((object?)null).ToApiResponse("If the email exists, an OTP has been sent."));
    }

    [EndpointSummary("Verify OTP")]
    [EndpointDescription("Verifies a one-time password previously issued by /auth/forgot-password.")]
    public static async Task<Results<Ok<ApiResponse<object?>>, JsonHttpResult<ApiErrorResponse>>> VerifyOtp(
        HttpContext httpContext,
        UserManager<ApplicationUser> userManager,
        OtpVerifyRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        var valid = user is not null &&
            await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "reset-password", request.Otp);

        if (!valid)
        {
            return TypedResults.Json(
                new ApiErrorResponse
                {
                    ErrorCode = "VALIDATION_ERROR",
                    Message = "The OTP is invalid or has expired.",
                    RequestId = httpContext.TraceIdentifier,
                    StatusCode = StatusCodes.Status400BadRequest
                },
                statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Ok(((object?)null).ToApiResponse("OTP verified."));
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

    private static ApiErrorResponse UnauthorizedError(HttpContext httpContext, string message) => new()
    {
        ErrorCode = "UNAUTHORIZED",
        Message = message,
        RequestId = httpContext.TraceIdentifier,
        StatusCode = StatusCodes.Status401Unauthorized
    };

    private record BearerTokenPayload(string AccessToken, int ExpiresIn);
}
