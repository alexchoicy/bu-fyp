using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("student_programmes")]
[PrimaryKey(nameof(StudentId), nameof(ProgrammeVersionId))]
public class StudentProgramme
{
    [Required]
    [Column("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [ForeignKey(nameof(StudentId))]
    public User Student { get; set; } = null!;

    [Required]
    [Column("programme_version_id")]
    public int ProgrammeVersionId { get; set; }

    [ForeignKey(nameof(ProgrammeVersionId))]
    public ProgrammeVersion ProgrammeVersion { get; set; } = null!;
}
