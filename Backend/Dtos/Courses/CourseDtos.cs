namespace Backend.Dtos.Courses;

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
    public string CourseCode { get; set; } = string.Empty;
    public string NoOfUnits { get; set; } = string.Empty;
    public string OfferingDepartment { get; set; } = string.Empty;
    public string Prerequisites { get; set; } = string.Empty;
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