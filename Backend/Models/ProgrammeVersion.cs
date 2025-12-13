using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("programme_versions")]
public class ProgrammeVersion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("programme_id")]
    public int ProgrammeId { get; set; }

    [ForeignKey(nameof(ProgrammeId))]
    public Programme Programme { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("last_edited_at")]
    public DateTime LastEditedAt { get; set; } = DateTime.UtcNow;
    [Column("version_number")]
    public int VersionNumber { get; set; }

    [Required]
    [Column("start_year")]
    public int StartYear { get; set; }

    [Required]
    [Column("end_year")]
    public int? EndYear { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Column("total_credits")]
    public required int TotalCredits { get; set; }
    

    // Navigation properties
    public ICollection<StudentProgramme> StudentProgrammes { get; set; } = new List<StudentProgramme>();
    public ICollection<ProgrammeCategory> ProgrammeCategories { get; set; } = new List<ProgrammeCategory>();
}
