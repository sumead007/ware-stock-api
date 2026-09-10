using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Settings.Commands.UpdateDisplaySettings;

[Authorize]
public record UpdateDisplaySettingsCommand : IRequest<UserSettingsDto>
{
    public IReadOnlyCollection<SidebarItem> Items { get; init; } = [];
}

public class UpdateDisplaySettingsCommandValidator : AbstractValidator<UpdateDisplaySettingsCommand>
{
    public UpdateDisplaySettingsCommandValidator()
    {
        RuleFor(v => v.Items).NotEmpty();
    }
}

public class UpdateDisplaySettingsCommandHandler : IRequestHandler<UpdateDisplaySettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public UpdateDisplaySettingsCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateDisplaySettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;

        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);
        settings.DisplayItems = request.Items.ToList();
        await _context.SaveChangesAsync(cancellationToken);

        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);

        return UserSettingsDto.From(user, settings);
    }
}
