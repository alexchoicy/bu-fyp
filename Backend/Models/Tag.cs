using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum TagType
{
    Domain,
    Skill,
    ContentType
}

[Table("tags")]
public class Tag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("tag_type")]
    public required TagType TagType { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("embedding")]
    public float[]? Embedding { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
}

