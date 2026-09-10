using WareStockApi.Application.Common.Interfaces;

namespace WareStockApi.Application.Users.Commands.BulkDeleteUsers;

public record BulkDeleteUsersCommand(IReadOnlyCollection<string> Ids) : IRequest;

public class BulkDeleteUsersCommandValidator : AbstractValidator<BulkDeleteUsersCommand>
{
    public BulkDeleteUsersCommandValidator()
    {
        RuleFor(v => v.Ids).NotEmpty();
    }
}

public class BulkDeleteUsersCommandHandler : IRequestHandler<BulkDeleteUsersCommand>
{
    private readonly IIdentityService _identityService;

    public BulkDeleteUsersCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(BulkDeleteUsersCommand request, CancellationToken cancellationToken)
    {
        await _identityService.DeleteUsersAsync(request.Ids, cancellationToken);
    }
}
