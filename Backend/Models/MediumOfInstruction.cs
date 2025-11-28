using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("medium_of_instructions")]
public class MediumOfInstruction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public required string Name { get; set; }

    // Navigation property
    public ICollection<CourseVersionMedium> CourseVersionMediums { get; set; } = new List<CourseVersionMedium>();
}
