using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("course_groups")]
public class CourseGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<CategoryGroup> CategoryGroups { get; set; } = new List<CategoryGroup>();
    public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
}
