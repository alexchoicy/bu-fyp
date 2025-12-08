namespace Backend.Dtos.Courses;

using Backend.Dtos.Facts;
using Backend.Models;

public class CreateCourseDto
{
    public required string Name { get; set; }
    public required string CourseNumber { get; set; }
    public required int CodeId { get; set; }
    public int Credit { get; set; } = 0;
    public string? Description { get; set; }
    public List<int> GroupIds { get; set; } = new();
    public List<int> DepartmentIds { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class CreateCourseVersionDto
{
    public string Description { get; set; } = string.Empty;
    public string AimAndObjectives { get; set; } = string.Empty;
    public string CourseContent { get; set; } = string.Empty;
    public List<CILOs> CILOs { get; set; } = new();
    public List<TLAs> TLAs { get; set; } = new();
    public List<CreateAssessmentDto> Assessments { get; set; } = new();
    public List<int> MediumOfInstructionIds { get; set; } = new();
    public List<int> PreRequisiteCourseVersionIds { get; set; } = new();
    public List<int> AntiRequisiteCourseVersionIds { get; set; } = new();
    public required int FromYear { get; set; }
    public required int FromTermId { get; set; }
    public int? ToYear { get; set; }
    public int? ToTermId { get; set; }
}

public class CreateAssessmentDto
{
    public required string Name { get; set; }
    public decimal Weighting { get; set; }
    public AssessmentCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}


public class CourseCodeDto
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseCodeNumber { get; set; } = string.Empty;
}

public class PdfParseResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public long Size { get; set; }
    public int Pages { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public ParsedSectionsDto ParsedSections { get; set; } = new();
}

public class ParsedSectionsDto
{
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCodeNumber { get; set; } = string.Empty;
    //nah i give the raw back to frontend and find the code there
    public string CourseCode { get; set; } = string.Empty;
    public string NoOfUnits { get; set; } = string.Empty;
    //nah this too
    public string OfferingDepartment { get; set; } = string.Empty;
    public List<CourseCodeDto> Prerequisites { get; set; } = new();
    public string MediumOfInstruction { get; set; } = string.Empty;
    public string AimsObjectives { get; set; } = string.Empty;
    public string CourseContent { get; set; } = string.Empty;
    public string CilosRaw { get; set; } = string.Empty;
    public List<CILOs> CILOs { get; set; } = new();
    public string TlasRaw { get; set; } = string.Empty;
    public List<TLAs> TLAs { get; set; } = new();
    public string AssessmentMethodsRaw { get; set; } = string.Empty;
    public List<AssessmentMethod> AssessmentMethods { get; set; } = new();
}


public class CourseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public int CodeId { get; set; }
    public string CodeTag { get; set; } = string.Empty;
    public int Credit { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<CourseVersionResponseDto> Versions { get; set; } = new();
    public List<string> Departments { get; set; } = new();
}

public class CourseVersionResponseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AimAndObjectives { get; set; } = string.Empty;
    public string CourseContent { get; set; } = string.Empty;
    public List<CILOs> CILOs { get; set; } = new();
    public List<TLAs> TLAs { get; set; } = new();
    public int VersionNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    // Academic year + term range (e.g., 2024 Sem1 to 2025 Sem1)
    public int FromYear { get; set; }
    public TermResponseDto FromTerm { get; set; } = null!;
    public int? ToYear { get; set; }
    public TermResponseDto? ToTerm { get; set; }

    public List<AssessmentResponseDto> Assessments { get; set; } = new();
    public List<string> MediumOfInstruction { get; set; } = new();
}

public class AssessmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weighting { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SimpleCourseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public int CodeId { get; set; }
    public string CodeTag { get; set; } = string.Empty;
    public CourseVersionResponseDto? MostRecentVersion { get; set; }
}
