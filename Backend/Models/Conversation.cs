namespace Backend.Models;

public class Conversation
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
