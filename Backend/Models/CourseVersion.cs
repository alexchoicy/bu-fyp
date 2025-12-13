using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class CILOs
{
    public required string code { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TLAs
{
    public required string[] code { get; set; }
    public string Description { get; set; } = string.Empty;
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

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("aim_and_objectives")]
    public string AimAndObjectives { get; set; } = string.Empty;

    [Column("course_content")]
    public string CourseContent { get; set; } = string.Empty;

    [Column("version_number")]
    public int VersionNumber { get; set; }

    [Column("cilos")]
    public List<CILOs> CILOs { get; set; } = new();

    [Column("tlas")]
    public List<TLAs> TLAs { get; set; } = new();

    // year = 2024 = 2024-2025 academic year
    [Column("from_year")]
    public int FromYear { get; set; }

    [Column("from_term_id")]
    public int FromTermId { get; set; }

    [ForeignKey(nameof(FromTermId))]
    public Term? FromTerm { get; set; }

    [Column("to_year")]
    public int? ToYear { get; set; } // null means ongoing/current

    [Column("to_term_id")]
    public int? ToTermId { get; set; } // null means ongoing/current

    [ForeignKey(nameof(ToTermId))]
    public Term? ToTerm { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseVersionMedium> CourseVersionMediums { get; set; } = new List<CourseVersionMedium>();
    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    public ICollection<CoursePreReq> Prerequisites { get; set; } = new List<CoursePreReq>();
    public ICollection<CoursePreReq> PrerequisiteFor { get; set; } = new List<CoursePreReq>();
    public ICollection<CourseAntiReq> AntiRequisites { get; set; } = new List<CourseAntiReq>();
    public ICollection<CourseAntiReq> AntiRequisiteFor { get; set; } = new List<CourseAntiReq>();
    public ICollection<CourseAssessment> Assessments { get; set; } = new List<CourseAssessment>();
}
