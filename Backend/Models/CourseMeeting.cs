using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("course_meetings")]
public class CourseMeeting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("section_id")]
    public int SectionId { get; set; }

    [ForeignKey(nameof(SectionId))]
    public CourseSection CourseSection { get; set; } = null!;

    [Column("meeting_type")]
    public string MeetingType { get; set; } = string.Empty;

    [Column("day")]
    public int Day { get; set; }

    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly EndTime { get; set; }
}
