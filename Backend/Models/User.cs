using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models;

// The email, password and roles are handled by Identity Framework
[Table("users")]
public class User : IdentityUser
{

    [Column("name")]
    public string Name { get; set; } = string.Empty;
    public ICollection<StudentProgramme> StudentProgrammes { get; set; } = new List<StudentProgramme>();
}
