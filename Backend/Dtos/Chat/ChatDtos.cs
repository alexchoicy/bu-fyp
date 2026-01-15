using Backend.Models;

namespace Backend.Dtos.Chat;

public class CreateRoomResponseDto
{
    public string RoomId { get; set; } = string.Empty;
}

public class MessageResponseDto
{
    public string Id { get; set; } = string.Empty;
    public MessageRole Role { get; set; } = MessageRole.User;
    public string Content { get; set; } = string.Empty;
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
    public DateTime CreatedAt { get; set; }
}

public class ChatHistoryResponseDto
{
    public List<MessageResponseDto> Messages { get; set; } = new();
}

public class SendMessageResponseDto
{
    public string GeneratedId { get; set; } = string.Empty;
}

