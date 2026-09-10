using WareStockApi.Application.Common.Interfaces;

namespace WareStockApi.Application.Chats.Queries.GetConversations;

public record GetConversationsQuery(string? Search) : IRequest<List<ConversationDto>>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetConversationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Conversations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.FullName.Contains(request.Search));
        }

        return await query
            .OrderByDescending(c => c.LastMessageAt)
            .ProjectTo<ConversationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
