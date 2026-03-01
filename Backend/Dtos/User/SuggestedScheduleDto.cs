namespace Backend.Dtos.User;

public class SuggestedScheduleItemDto
{
    public int Id { get; set; }
    public int StudyYear { get; set; }
    public int TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseNumber { get; set; }
    public string? CourseName { get; set; }
    public int? CourseCredit { get; set; }
    public bool IsCoreElective { get; set; }
    public bool IsFreeElective { get; set; }
    public decimal? Credits { get; set; }
    public string ItemType { get; set; } = "Course";
}

public class SuggestedScheduleTermDto
{
    public int TermId { get; set; }
    public string TermName { get; set; } = string.Empty;
    public List<SuggestedScheduleItemDto> Items { get; set; } = new();
}

public class SuggestedScheduleYearDto
{
    public int StudyYear { get; set; }
    public List<SuggestedScheduleTermDto> Terms { get; set; } = new();
}

public class SuggestedScheduleResponseDto
{
    public int CurrentStudyYear { get; set; }
    public int CurrentTermId { get; set; }
    public string CurrentTermName { get; set; } = string.Empty;
    public List<SuggestedScheduleYearDto> Years { get; set; } = new();
}
