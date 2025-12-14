using System.Text.Json.Serialization;
using Backend.Models;

namespace Backend.Dtos.Programmes;

/// <summary>
/// Root DTO for user's programme detail including all categories, groups, and courses
/// </summary>
public class UserProgrammeDetailDto
{
    public int ProgrammeVersionId { get; set; }
    public int ProgrammeId { get; set; }
    public string ProgrammeName { get; set; } = string.Empty;
    public string ProgrammeDescription { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public bool IsActive { get; set; }
    public int TotalCredits { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastEditedAt { get; set; }

    public List<ProgrammeCategoryDetailDto> Categories { get; set; } = new();
}

/// <summary>
/// Represents a category within a programme version
/// </summary>
public class ProgrammeCategoryDetailDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CategoryType Type { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int MinCredit { get; set; }
    public int Priority { get; set; }
    public string? RulesJson { get; set; }

    public List<CategoryGroupDetailDto> Groups { get; set; } = new();
}

/// <summary>
/// Represents a group within a category
/// </summary>
public class CategoryGroupDetailDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;

    public List<GroupCourseDetailDto> GroupCourses { get; set; } = new();
}

/// <summary>
/// Represents a course or code within a group
/// </summary>
public class GroupCourseDetailDto
{
    public int GroupCourseId { get; set; }
    
    /// <summary>
    /// If this group course is a Course, this will be populated
    /// </summary>
    public SimpleGroupCourseDto? Course { get; set; }
    
    /// <summary>
    /// If this group course is a Code, this will be populated
    /// </summary>
    public SimpleCodeDto? Code { get; set; }
}

/// <summary>
/// Simple course info without deep nesting into CourseVersions
/// </summary>
public class SimpleGroupCourseDto
{
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public int Credit { get; set; }
    public string CodeTag { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Simple code info with name and tag
/// </summary>
public class SimpleCodeDto
{
    public int CodeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
}

