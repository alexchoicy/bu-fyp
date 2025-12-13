using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("student_courses")]
[PrimaryKey(nameof(StudentId), nameof(CourseId))]
public class StudentCourse
{
    [Required]
    [Column("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [ForeignKey(nameof(StudentId))]
    public User Student { get; set; } = null!;

    [Required]
    [Column("course_id")]
    public int CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    [Required]
    [Column("term_id")]
    public int TermId { get; set; }

    [ForeignKey(nameof(TermId))]
    public Term Term { get; set; } = null!;

    [Required]
    [Column("academic_year")]
    public int AcademicYear { get; set; }

    [Column("grade")]
    public Grade? Grade { get; set; }

    [Required]
    [Column("status")]
    public StudentCourseStatus Status { get; set; } = StudentCourseStatus.Enrolled;

    [Column("notes")]
    public string Notes { get; set; } = string.Empty;
}
