using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("programme_categories")]
[PrimaryKey(nameof(ProgrammeVersionId), nameof(CategoryId))]
public class ProgrammeCategory
{
    [Column("programme_version_id")]
    public int ProgrammeVersionId { get; set; }

    [ForeignKey(nameof(ProgrammeVersionId))]
    public ProgrammeVersion ProgrammeVersion { get; set; } = null!;

    [Column("category_id")]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}
