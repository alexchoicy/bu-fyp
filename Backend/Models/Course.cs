using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("courses")]
public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("code_id")]
    public int CodeId { get; set; }

    [ForeignKey(nameof(CodeId))]
    public Code Code { get; set; } = null!;

    [Column("course_number")]
    public required string CourseNumber { get; set; }

    public ICollection<CourseVersion> CourseVersions { get; set; } = new List<CourseVersion>();
    public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
    public ICollection<CourseDepartment> CourseDepartments { get; set; } = new List<CourseDepartment>();
}
