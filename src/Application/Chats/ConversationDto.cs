using WareStockApi.Domain.Entities;

namespace WareStockApi.Application.Chats;

public class ConversationDto
{
    public string Id { get; init; } = string.Empty;

    public string ParticipantId { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Profile { get; init; } = string.Empty;

    public DateTimeOffset? LastMessageAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Conversation, ConversationDto>();
        }
    }
}
