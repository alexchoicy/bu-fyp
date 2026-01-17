using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("programme_suggested_courses_schedule")]
public class ProgrammeSuggestedCourseSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("programme_version_id")]
    public int ProgrammeVersionId { get; set; }

    [ForeignKey(nameof(ProgrammeVersionId))]
    public ProgrammeVersion ProgrammeVersion { get; set; } = null!;

    // Study year within the programme (e.g., 1..4+)
    [Required]
    [Column("study_year")]
    public int StudyYear { get; set; }

    [Required]
    [Column("term_id")]
    public int TermId { get; set; }

    [ForeignKey(nameof(TermId))]
    public Term Term { get; set; } = null!;

    // Nullable for free-elective records
    [Column("course_id")]
    public int? CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    // Indicates if this is a free-elective slot
    [Required]
    [Column("is_free_elective")]
    public bool IsFreeElective { get; set; } = false;

    // Credits for this entry (especially important for free-electives)
    [Column("credits")]
    public decimal? Credits { get; set; }

    public bool IsCoreElective { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
