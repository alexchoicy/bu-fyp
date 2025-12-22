using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace Backend.Models;

[Table("policy_section_chunks")]
public class PolicySectionChunk
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("chunk_key")]
    public long ChunkKey { get; set; }

    [Column("policy_section_key")]
    public long PolicySectionKey { get; set; }

    [ForeignKey(nameof(PolicySectionKey))]
    public PolicySection PolicySection { get; set; } = null!;

    [Column("chunk_index")]
    public int ChunkIndex { get; set; }

    [Column("content")]
    public required string Content { get; set; }

    [Column("embedding", TypeName = "vector(3072)")]
    public Vector? Embedding { get; set; }

    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

