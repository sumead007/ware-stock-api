using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Settings.Commands.UpdateNotificationSettings;

[Authorize]
public record UpdateNotificationSettingsCommand : IRequest<UserSettingsDto>
{
    public NotificationType Type { get; init; }

    public bool Mobile { get; init; }

    public bool CommunicationEmails { get; init; }

    public bool SocialEmails { get; init; }

    public bool MarketingEmails { get; init; }

    /// <summary>Always forced to <see langword="true"/> — the UI never allows disabling it.</summary>
    public bool SecurityEmails { get; init; } = true;
}

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(v => v.Type).IsInEnum();
    }
}

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public UpdateNotificationSettingsCommandHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;

        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);
        settings.NotificationType = request.Type;
        settings.Mobile = request.Mobile;
        settings.CommunicationEmails = request.CommunicationEmails;
        settings.SocialEmails = request.SocialEmails;
        settings.MarketingEmails = request.MarketingEmails;
        settings.SecurityEmails = true; // forced, regardless of what the client sends
        await _context.SaveChangesAsync(cancellationToken);

        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);

        return UserSettingsDto.From(user, settings);
    }
}
