using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("course_anti_reqs")]
[PrimaryKey(nameof(CourseVersionId), nameof(ExcludedCourseVersionId))]
public class CourseAntiReq
{
    [Column("course_version_id")]
    public int CourseVersionId { get; set; }

    [ForeignKey(nameof(CourseVersionId))]
    public CourseVersion CourseVersion { get; set; } = null!;

    [Column("excluded_course_version_id")]
    public int ExcludedCourseVersionId { get; set; }

    [ForeignKey(nameof(ExcludedCourseVersionId))]
    public CourseVersion ExcludedCourseVersion { get; set; } = null!;
}
