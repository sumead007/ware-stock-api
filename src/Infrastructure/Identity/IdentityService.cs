using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WareStockApi.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    // ----- Users domain -----

    public async Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await Query().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<PaginatedList<UserDto>> GetUsersAsync(
        int page,
        int pageSize,
        IReadOnlyCollection<UserStatus>? statuses,
        string? username,
        CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking().AsQueryable();

        if (statuses is { Count: > 0 })
        {
            query = query.Where(u => statuses.Contains(u.Status));
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(u => u.UserName != null && u.UserName.Contains(username));
        }

        var projected = query
            .OrderByDescending(u => u.CreatedAt)
            .Select(ToDtoExpression);

        return await PaginatedList<UserDto>.CreateAsync(projected, page, pageSize, cancellationToken);
    }

    public async Task<(Result Result, UserDto? User)> CreateUserAsync(
        string firstName,
        string lastName,
        string username,
        string email,
        string phoneNumber,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            PhoneNumber = phoneNumber,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = $"{firstName} {lastName}".Trim(),
            Status = UserStatus.Active
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), result.Succeeded ? ToDto(user) : null);
    }

    public async Task<(Result Result, UserDto? User)> UpdateUserAsync(
        string userId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        UserStatus status,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return (Result.Failure(["User not found."]), null);
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.DisplayName = $"{firstName} {lastName}".Trim();
        user.Status = status;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _userManager.SetEmailAsync(user, email);
            if (!emailResult.Succeeded)
            {
                return (emailResult.ToApplicationResult(), null);
            }
        }

        if (!string.Equals(user.PhoneNumber, phoneNumber, StringComparison.Ordinal))
        {
            await _userManager.SetPhoneNumberAsync(user, phoneNumber);
        }

        if (!string.IsNullOrEmpty(password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, password);
            if (!passwordResult.Succeeded)
            {
                return (passwordResult.ToApplicationResult(), null);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);

        return (updateResult.ToApplicationResult(), updateResult.Succeeded ? ToDto(user) : null);
    }

    public async Task<Result> DeleteUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user is null ? Result.Success() : (await _userManager.DeleteAsync(user)).ToApplicationResult();
    }

    public async Task<Result> DeleteUsersAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        foreach (var id in ids)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) continue;

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
            }
        }

        return errors.Count == 0 ? Result.Success() : Result.Failure(errors);
    }

    public async Task<int> SetUsersStatusAsync(IReadOnlyCollection<string> ids, UserStatus status, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            user.Status = status;
            await _userManager.UpdateAsync(user);
        }

        return users.Count;
    }

    // ----- Dashboard -----

    public async Task<UserCounts> GetUserCountsAsync(CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users.AsNoTracking();

        var total = await users.CountAsync(cancellationToken);
        var active = await users.CountAsync(u => u.Status == UserStatus.Active, cancellationToken);
        var suspended = await users.CountAsync(u => u.Status == UserStatus.Suspended, cancellationToken);

        return new UserCounts(total, active, suspended);
    }

    public async Task<IReadOnlyList<UserDto>> GetRecentUsersAsync(int count, CancellationToken cancellationToken = default) =>
        await Query()
            .OrderByDescending(u => u.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    // ----- Settings (/me) -----

    public async Task<Result> UpdateProfileAsync(string userId, string name, string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure(["User not found."]);

        user.DisplayName = name;

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _userManager.SetEmailAsync(user, email);
            if (!emailResult.Succeeded) return emailResult.ToApplicationResult();
        }

        return (await _userManager.UpdateAsync(user)).ToApplicationResult();
    }

    public async Task<Result> UpdateDisplayNameAsync(string userId, string name, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure(["User not found."]);

        user.DisplayName = name;

        return (await _userManager.UpdateAsync(user)).ToApplicationResult();
    }

    private IQueryable<UserDto> Query() => _userManager.Users.AsNoTracking().Select(ToDtoExpression);

    private static readonly System.Linq.Expressions.Expression<Func<ApplicationUser, UserDto>> ToDtoExpression = u => new UserDto
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Username = u.UserName ?? string.Empty,
        Email = u.Email ?? string.Empty,
        PhoneNumber = u.PhoneNumber ?? string.Empty,
        DisplayName = u.DisplayName,
        Status = u.Status,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };

    private static UserDto ToDto(ApplicationUser u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Username = u.UserName ?? string.Empty,
        Email = u.Email ?? string.Empty,
        PhoneNumber = u.PhoneNumber ?? string.Empty,
        DisplayName = u.DisplayName,
        Status = u.Status,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
