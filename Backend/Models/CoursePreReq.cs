using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("course_pre_reqs")]
[PrimaryKey(nameof(CourseVersionId), nameof(RequiredCourseVersionId))]
public class CoursePreReq
{
    [Column("course_version_id")]
    public int CourseVersionId { get; set; }

    [ForeignKey(nameof(CourseVersionId))]
    public CourseVersion CourseVersion { get; set; } = null!;

    [Column("required_course_version_id")]
    public int RequiredCourseVersionId { get; set; }

    [ForeignKey(nameof(RequiredCourseVersionId))]
    public CourseVersion RequiredCourseVersion { get; set; } = null!;
}
