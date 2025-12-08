namespace Backend.Dtos.Facts;

public class CurrentFactsResponseDto
{
    public int CurrentSemester { get; set; }
    public int CurrentAcademicYear { get; set; }
    public string? CurrentTermName { get; set; }
}

public class CodeResponseDto
{
    public int Id { get; set; }
    public string Tag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CourseGroupResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TermResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
