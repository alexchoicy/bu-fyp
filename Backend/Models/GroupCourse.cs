using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("group_courses")]
public class GroupCourse
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("group_id")]
    public int GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public CourseGroup Group { get; set; } = null!;

    [Column("course_id")]
    public int? CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [Column("code_id")]
    public int? CodeId { get; set; }

    [ForeignKey(nameof(CodeId))]
    public Code? Code { get; set; }
}
