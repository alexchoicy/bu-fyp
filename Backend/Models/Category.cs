using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Backend.Models;

public enum CategoryType
{
    Core,
    Elective,
    GE
}

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
    public string? RulesJson { get; set; }
    
    [Column("notes")]
    public string Notes { get; set; } = string.Empty;

    [Column("min_credit")]
    public int MinCredit { get; set; } = 0;

    [Column("priority")]
    public int Priority { get; set; } = 0;

    [Column("type")]
    public CategoryType Type { get; set; } = CategoryType.Core;

    // Navigation properties
    public ICollection<ProgrammeCategory> ProgrammeCategories { get; set; } = new List<ProgrammeCategory>();
    public ICollection<CategoryGroup> CategoryGroups { get; set; } = new List<CategoryGroup>();
    
    [NotMapped]
    public RuleNode? Rules
    {
        get
        {
            if (string.IsNullOrEmpty(RulesJson))
                return null;
            return JsonSerializer.Deserialize<RuleNode>(RulesJson);
        }
        set
        {
            RulesJson = value == null
                ? null
                : JsonSerializer.Serialize<RuleNode>(value);
        }
    }
    
}
