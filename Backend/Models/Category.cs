using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("categories")]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("rules", TypeName = "jsonb")]
    public RuleNode? Rules { get; set; }

    [Column("notes")]
    public string Notes { get; set; } = string.Empty;

    [Column("min_credit")]
    public int MinCredit { get; set; } = 0;

    [Column("priority")]
    public int Priority { get; set; } = 0;

    // Navigation properties
    public ICollection<ProgrammeCategory> ProgrammeCategories { get; set; } = new List<ProgrammeCategory>();
    public ICollection<CategoryGroup> CategoryGroups { get; set; } = new List<CategoryGroup>();
}
