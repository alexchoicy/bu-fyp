using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("codes")]
public class Code
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("tag")]
    public string Tag { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
}
