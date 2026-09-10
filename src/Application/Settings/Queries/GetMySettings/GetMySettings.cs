using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Security;

namespace WareStockApi.Application.Settings.Queries.GetMySettings;

[Authorize]
public record GetMySettingsQuery : IRequest<UserSettingsDto>;

public class GetMySettingsQueryHandler : IRequestHandler<GetMySettingsQuery, UserSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public GetMySettingsQueryHandler(IApplicationDbContext context, IIdentityService identityService, IUser currentUser)
    {
        _context = context;
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async Task<UserSettingsDto> Handle(GetMySettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;
        var user = Ensure.Found(await _identityService.GetUserAsync(userId, cancellationToken), "User", userId);
        var settings = await UserSettingsHelper.GetOrCreateAsync(_context, userId, cancellationToken);

        return UserSettingsDto.From(user, settings);
    }
}
