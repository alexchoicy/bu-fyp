namespace Backend.Models;

public class Message
{
    public Guid Id { get; set; }
    public MessageRole Role { get; set; } = MessageRole.User;
    public string Content { get; set; } = string.Empty;
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to Conversation
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; }
}

