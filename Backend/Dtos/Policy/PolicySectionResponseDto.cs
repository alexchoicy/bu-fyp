namespace Backend.Dtos.Policy;

public class PolicySectionResponseDto
{
    public long PolicySectionKey { get; set; }
    public string SectionId { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string DocTitle { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
