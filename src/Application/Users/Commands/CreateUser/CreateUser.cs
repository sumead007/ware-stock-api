using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using FluentValidation.Results;

namespace WareStockApi.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<UserDto>
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConfirmPassword { get; init; } = string.Empty;
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.FirstName).NotEmpty();
        RuleFor(v => v.LastName).NotEmpty();
        RuleFor(v => v.Username).NotEmpty();
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.PhoneNumber).NotEmpty();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(8)
            .Matches("[a-z]").WithMessage("'{PropertyName}' must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("'{PropertyName}' must contain at least one digit.");
        RuleFor(v => v.ConfirmPassword).Equal(v => v.Password).WithMessage("Passwords do not match.");
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IIdentityService _identityService;

    public CreateUserCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, user) = await _identityService.CreateUserAsync(
            request.FirstName, request.LastName, request.Username, request.Email, request.PhoneNumber, request.Password, cancellationToken);

        if (!result.Succeeded || user is null)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.Email), e)));
        }

        return user;
    }
}
