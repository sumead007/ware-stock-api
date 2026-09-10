using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Settings.Commands.UpdateAccountSettings;

[Authorize]
public record UpdateAccountSettingsCommand : IRequest<UserSettingsDto>
{
    public string Name { get; init; } = string.Empty;

    public DateOnly Dob { get; init; }

    public Language Language { get; init; }
}

public class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MinimumLength(2).MaximumLength(30);
        RuleFor(v => v.Language).IsInEnum();
    }
}

public class UpdateAccountSettingsCommandHandler : IRequestHandler<UpdateAccountSettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public UpdateAccountSettingsCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateAccountSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;

        await _identityService.UpdateDisplayNameAsync(userId, request.Name, cancellationToken);

        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);
        settings.DateOfBirth = request.Dob;
        settings.Language = request.Language;
        await _context.SaveChangesAsync(cancellationToken);

        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);

        return UserSettingsDto.From(user, settings);
    }
}
