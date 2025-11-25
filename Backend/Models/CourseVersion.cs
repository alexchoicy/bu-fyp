using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

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

    // Navigation properties
    public ICollection<CourseSection> CourseSections { get; set; } = new List<CourseSection>();
    public ICollection<CoursePreReq> Prerequisites { get; set; } = new List<CoursePreReq>();
    public ICollection<CoursePreReq> PrerequisiteFor { get; set; } = new List<CoursePreReq>();
    public ICollection<CourseAntiReq> AntiRequisites { get; set; } = new List<CourseAntiReq>();
    public ICollection<CourseAntiReq> AntiRequisiteFor { get; set; } = new List<CourseAntiReq>();
}
