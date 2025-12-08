using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("course_sections")]
public class CourseSection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_version_id")]
    public int CourseVersionId { get; set; }

    [ForeignKey(nameof(CourseVersionId))]
    public CourseVersion CourseVersion { get; set; } = null!;

    [Column("year")]
    public int Year { get; set; }

    [Column("term_id")]
    public int TermId { get; set; }

    [ForeignKey(nameof(TermId))]
    public Term Term { get; set; } = null!;

    [Column("section_number")]
    public int SectionNumber { get; set; }

    // Navigation properties
    public ICollection<CourseMeeting> CourseMeetings { get; set; } = new List<CourseMeeting>();
}
