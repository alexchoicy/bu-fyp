using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum AssessmentCategory
{
    Examination = 1,
    Assignment = 2,
    Project = 3,
    GroupProject = 4,
    SoloProject = 5,
    Participation = 6,
    Presentation = 7,
    Other = 99
}

// This is only used for AI extraction
public class AssessmentMethod
{
    public required string Name { get; set; }
    public decimal Weighting { get; set; }
    public AssessmentCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}


[Table("course_assessments")]
public class CourseAssessment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_version_id")]
    public int CourseVersionId { get; set; }

    [ForeignKey(nameof(CourseVersionId))]
    public CourseVersion CourseVersion { get; set; } = null!;

    [Column("name")]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("weighting")]
    [Range(0, 100)]
    public decimal Weighting { get; set; }

    [Column("category")]
    public AssessmentCategory Category { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;
}
