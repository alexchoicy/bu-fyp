using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("course_version_mediums")]
public class CourseVersionMedium
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("course_version_id")]
    public int CourseVersionId { get; set; }

    [ForeignKey(nameof(CourseVersionId))]
    public CourseVersion CourseVersion { get; set; } = null!;

    [Column("medium_of_instruction_id")]
    public int MediumOfInstructionId { get; set; }

    [ForeignKey(nameof(MediumOfInstructionId))]
    public MediumOfInstruction MediumOfInstruction { get; set; } = null!;
}
