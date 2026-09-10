using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Settings.Commands.UpdateAppearanceSettings;

[Authorize]
public record UpdateAppearanceSettingsCommand : IRequest<UserSettingsDto>
{
    public Theme Theme { get; init; }

    public AppFont Font { get; init; }
}

public class UpdateAppearanceSettingsCommandValidator : AbstractValidator<UpdateAppearanceSettingsCommand>
{
    public UpdateAppearanceSettingsCommandValidator()
    {
        RuleFor(v => v.Theme).IsInEnum();
        RuleFor(v => v.Font).IsInEnum();
    }
}

public class UpdateAppearanceSettingsCommandHandler : IRequestHandler<UpdateAppearanceSettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public UpdateAppearanceSettingsCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateAppearanceSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;

        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);
        settings.Theme = request.Theme;
        settings.Font = request.Font;
        await _context.SaveChangesAsync(cancellationToken);

        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);

        return UserSettingsDto.From(user, settings);
    }
}
