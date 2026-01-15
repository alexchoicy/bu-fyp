namespace Backend.Dtos.Policy;

public class PolicySearchResultDto
{
    public PolicySectionChunkResponseDto SectionChunk { get; set; } = null!;
    public double Similarity { get; set; }
}
