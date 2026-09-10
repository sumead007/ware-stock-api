namespace WareStockApi.Domain.Entities;

public class Conversation : BaseEntity
{
    /// <summary>References the counterpart user's id (User.Id).</summary>
    public string ParticipantId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>Job title / topic of the counterpart.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Profile picture URL.</summary>
    public string Profile { get; set; } = string.Empty;

    public DateTimeOffset? LastMessageAt { get; set; }

    public IList<Message> Messages { get; private set; } = new List<Message>();
}
