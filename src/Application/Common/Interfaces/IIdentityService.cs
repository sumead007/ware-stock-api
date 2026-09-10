using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    // ----- Users domain (GET/POST/PUT/DELETE /users) -----

    Task<UserDto?> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<PaginatedList<UserDto>> GetUsersAsync(
        int page,
        int pageSize,
        IReadOnlyCollection<UserStatus>? statuses,
        string? username,
        CancellationToken cancellationToken = default);

    Task<(Result Result, UserDto? User)> CreateUserAsync(
        string firstName,
        string lastName,
        string username,
        string email,
        string phoneNumber,
        string password,
        CancellationToken cancellationToken = default);

    Task<(Result Result, UserDto? User)> UpdateUserAsync(
        string userId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string? password,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<Result> DeleteUsersAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken = default);

    Task<int> SetUsersStatusAsync(IReadOnlyCollection<string> ids, UserStatus status, CancellationToken cancellationToken = default);

    // ----- Dashboard -----

    Task<UserCounts> GetUserCountsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetRecentUsersAsync(int count, CancellationToken cancellationToken = default);

    // ----- Settings (/me) -----

    Task<Result> UpdateProfileAsync(string userId, string name, string email, CancellationToken cancellationToken = default);

    Task<Result> UpdateDisplayNameAsync(string userId, string name, CancellationToken cancellationToken = default);
}
