using System.Text.Json.Serialization;

namespace Backend.Dtos.Chat;

public class MessageRelatedDataResponseDto
{
    public List<string> NextSuggestions { get; set; } = new();

    [JsonPropertyName("timetable_tool")]
    public List<TimetableGenerationRequestDto> TimetableTool { get; set; } = new();
}