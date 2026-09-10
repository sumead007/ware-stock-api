using ValidationException = WareStockApi.Application.Common.Exceptions.ValidationException;
using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;
using FluentValidation.Results;

namespace WareStockApi.Application.Settings.Commands.UpdateProfileSettings;

[Authorize]
public record UpdateProfileSettingsCommand : IRequest<UserSettingsDto>
{
    public string Name { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public class UpdateProfileSettingsCommandValidator : AbstractValidator<UpdateProfileSettingsCommand>
{
    public UpdateProfileSettingsCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MinimumLength(2).MaximumLength(30);
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
    }
}

public class UpdateProfileSettingsCommandHandler : IRequestHandler<UpdateProfileSettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public UpdateProfileSettingsCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateProfileSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;

        var result = await _identityService.UpdateProfileAsync(userId, request.Name, request.Email, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(e => new ValidationFailure(nameof(request.Email), e)));
        }

        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);
        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);

        return UserSettingsDto.From(user, settings);
    }
}
