using System.Text.Json.Serialization;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Common.Models;

/// <summary>
/// The shape in which <see cref="Interfaces.IIdentityService"/> exposes user data to the
/// Application layer, which never references <c>ApplicationUser</c> (an Infrastructure type)
/// directly.
/// </summary>
public class UserDto
{
    public required string Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }

    public required string PhoneNumber { get; init; }

    /// <summary>
    /// Not part of the public `User` schema (only `firstName`/`lastName` are) — used internally to
    /// compose the Settings `profile.name`/`account.name` fields and the Auth `user.name` field.
    /// </summary>
    [JsonIgnore]
    public required string DisplayName { get; init; }

    public UserStatus Status { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}

public record UserCounts(int Total, int Active, int Invited, int Suspended);
