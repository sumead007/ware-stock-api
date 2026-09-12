using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Domain.Enums;
using FluentValidation.Results;

namespace WareStockApi.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<UserDto>
{
    public string Id { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public UserStatus Status { get; init; }

    public string? Password { get; init; }

    public string? ConfirmPassword { get; init; }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty();
        RuleFor(v => v.LastName).NotEmpty();
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.PhoneNumber).NotEmpty();

        When(v => !string.IsNullOrEmpty(v.Password), () =>
        {
            RuleFor(v => v.Password).MinimumLength(8)
                .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least one digit.");
            RuleFor(v => v.ConfirmPassword).Equal(v => v.Password).WithMessage("Passwords do not match.");
        });
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IIdentityService _identityService;

    public UpdateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.UpdateUserAsync(
            request.Id, request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.Status, request.Password, cancellationToken);

        if (!result.Succeeded || user is null)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.Email), e)));
        }

        return user;
    }
}
