using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("policy_sections")]
public class PolicySection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("policy_section_key")]
    public long PolicySectionKey { get; set; }

    [Column("section_id")]
    [MaxLength(100)]
    public required string SectionId { get; set; }

    [Column("heading")]
    public required string Heading { get; set; }

    [Column("doc_title")]
    public required string DocTitle { get; set; }

    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<PolicySectionChunk> Chunks { get; set; } = new List<PolicySectionChunk>();
}

