using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Table("category_groups")]
[PrimaryKey(nameof(CategoryId), nameof(GroupId))]
public class CategoryGroup
{
    [Column("category_id")]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [Column("group_id")]
    public int? GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public CourseGroup Group { get; set; } = null!;

}
