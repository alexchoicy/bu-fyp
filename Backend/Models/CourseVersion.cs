using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class CILOs
{
    public required string code { get; set; }
    public string Description { get; set; } = string.Empty;
    public object? meta { get; set; } // For future use
}

public class TLAs
{
    public required string[] code { get; set; }
    public string Description { get; set; } = string.Empty;
    public object? meta { get; set; } // For future use
}

[Table("course_versions")]
public class CourseVersion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    [Column("credit")]
    public int Credit { get; set; } = 0;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("aim_and_objectives")]
    public string AimAndObjectives { get; set; } = string.Empty;

    [Column("course_content")]
    public string CourseContent { get; set; } = string.Empty;

    [Column("cilos", TypeName = "jsonb")]
    public CILOs[] CILOs { get; set; } = Array.Empty<CILOs>();

    [Column("tlas", TypeName = "jsonb")]
    public TLAs[] TLAs { get; set; } = Array.Empty<TLAs>();


    public ICollection<CourseVersionMedium> CourseVersionMediums { get; set; } = new List<CourseVersionMedium>();
    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    public ICollection<CoursePreReq> Prerequisites { get; set; } = new List<CoursePreReq>();
    public ICollection<CoursePreReq> PrerequisiteFor { get; set; } = new List<CoursePreReq>();
    public ICollection<CourseAntiReq> AntiRequisites { get; set; } = new List<CourseAntiReq>();
    public ICollection<CourseAntiReq> AntiRequisiteFor { get; set; } = new List<CourseAntiReq>();
    public ICollection<CourseAssessment> Assessments { get; set; } = new List<CourseAssessment>();
}
