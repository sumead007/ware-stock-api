using WareStockApi.Application.Common.Interfaces;
using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Chats.Commands.CreateConversation;

public record CreateConversationCommand(IReadOnlyCollection<string> ParticipantIds) : IRequest<ConversationDto>;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(v => v.ParticipantIds).NotEmpty();
    }
}

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;

    public CreateConversationCommandHandler(IApplicationDbContext context, IIdentityService identityService, IMapper mapper)
    {
        _context = context;
        _identityService = identityService;
        _mapper = mapper;
    }

    public async Task<ConversationDto> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var participantId = request.ParticipantIds.First();
        var participant = await _identityService.GetUserAsync(participantId, cancellationToken);

        var entity = new Conversation
        {
            ParticipantId = participantId,
            Username = participant?.Username ?? participantId,
            FullName = participant?.DisplayName ?? participantId,
            Title = participant?.Email ?? string.Empty,
            Profile = string.Empty
        };

        _context.Conversations.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ConversationDto>(entity);
    }
}
