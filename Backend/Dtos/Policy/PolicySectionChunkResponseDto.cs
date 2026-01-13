namespace Backend.Dtos.Policy;

public class PolicySectionChunkResponseDto
{
    public long ChunkKey { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public PolicySectionResponseDto PolicySection { get; set; } = null!;
}
