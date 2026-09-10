namespace WareStockApi.Domain.Entities;

public class Message : BaseEntity
{
    public string ConversationId { get; set; } = string.Empty;

    public string SenderId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
