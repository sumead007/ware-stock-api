using WareStockApi.Application.Common.Exceptions;
using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;

namespace WareStockApi.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(string Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IIdentityService _identityService;

    public GetUserByIdQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _identityService.GetUserAsync(request.Id, cancellationToken);

        return Ensure.Found(user, "User", request.Id);
    }
}
