namespace Backend.Dtos.Courses;

public class AIChatbotCourseSectionsDto
{
    public List<AIChatbotCourseSectionDto> Sections { get; set; } = new();
}

public class AIChatbotCourseSectionDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int TermId { get; set; }
    public int SectionNumber { get; set; }
    public List<AIChatbotCourseMeetingDto> Meetings { get; set; } = new();
}

public class AIChatbotCourseMeetingDto
{
    public int Id { get; set; }
    public string MeetingType { get; set; } = string.Empty;
    public int Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
