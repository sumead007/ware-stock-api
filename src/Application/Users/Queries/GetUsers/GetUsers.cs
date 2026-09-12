using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Application.Common.Models;
using WareStockApi.Application.Common.Validators;
using WareStockApi.Domain.Enums;

namespace WareStockApi.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<PaginatedList<UserDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public IReadOnlyCollection<UserStatus>? Status { get; init; }

    public string? Username { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).ValidPageSize();
        RuleFor(q => q.From)
            .LessThanOrEqualTo(q => q.To)
            .When(q => q.From.HasValue && q.To.HasValue)
            .WithMessage("'From' must be on or before 'To'.");
    }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _identityService.GetUsersAsync(request.Page, request.PageSize, request.Status, request.Username, request.From, request.To, cancellationToken);
}
