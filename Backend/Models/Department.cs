using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("departments")]
public class Department
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<CourseDepartment> CourseDepartments { get; set; } = new List<CourseDepartment>();
}
