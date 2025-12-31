namespace Backend.Dtos.Chat;

public class CreateRoomResponseDto
{
    public string RoomId { get; set; } = string.Empty;
}

public class MessageResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
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

public class MessageResultResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

