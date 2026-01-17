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

    [Column("entry_acedmic_year")]
    public int EntryAcedmicYear { get; set; }

    [Column("entry_year")]
    public int EntryYear { get; set; }
    public ICollection<StudentProgramme> StudentProgrammes { get; set; } = new List<StudentProgramme>();
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();


    public int GetCurrentStudyYear()
    {
        int currentYear = DateTime.Now.Year;

        if (DateTime.Now.Month < 9)
        {
            currentYear--;
        }

        int yearsSinceEntry = currentYear - EntryAcedmicYear;
        return EntryYear + yearsSinceEntry;
    }
}
