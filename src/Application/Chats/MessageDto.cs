using DomainMessage = WareStockApi.Domain.Entities.Message;

namespace WareStockApi.Application.Chats;

public class MessageDto
{
    public string Id { get; init; } = string.Empty;

    public string ConversationId { get; init; } = string.Empty;

    public string SenderId { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset Timestamp { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<DomainMessage, MessageDto>()
                .ForMember(d => d.Message, opt => opt.MapFrom(s => s.Content));
        }
    }
}
