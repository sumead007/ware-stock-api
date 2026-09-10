using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Users.Commands.BulkSetUserStatus;

public record BulkSetUserStatusCommand(IReadOnlyCollection<string> Ids, UserStatus Status) : IRequest<int>;

public class BulkSetUserStatusCommandValidator : AbstractValidator<BulkSetUserStatusCommand>
{
    public BulkSetUserStatusCommandValidator()
    {
        RuleFor(v => v.Ids).NotEmpty();
        RuleFor(v => v.Status).Must(s => s is UserStatus.Active or UserStatus.Inactive)
            .WithMessage("'{PropertyName}' must be either 'active' or 'inactive'.");
    }
}

public class BulkSetUserStatusCommandHandler : IRequestHandler<BulkSetUserStatusCommand, int>
{
    private readonly IIdentityService _identityService;

    public BulkSetUserStatusCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<int> Handle(BulkSetUserStatusCommand request, CancellationToken cancellationToken) =>
        _identityService.SetUsersStatusAsync(request.Ids, request.Status, cancellationToken);
}
