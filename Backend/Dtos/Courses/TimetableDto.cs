namespace Backend.Dtos.Courses;

public class TimetableResponseDto
{
    public int Year { get; set; }
    public string Term { get; set; } = string.Empty;
    public List<TimetableEntryDto> Entries { get; set; } = new();
}

public class TimetableEntryDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CodeTag { get; set; } = string.Empty;
    public string CourseNumber { get; set; } = string.Empty;
    public int Credit { get; set; }
    public int VersionId { get; set; }
    public int VersionNumber { get; set; }
    public List<TimetableSectionDto> Sections { get; set; } = new();
}

public class TimetableSectionDto
{
    public int SectionId { get; set; }
    public int SectionNumber { get; set; }
    public List<TimetableMeetingDto> Meetings { get; set; } = new();
}

public class TimetableMeetingDto
{
    public int Id { get; set; }
    public string MeetingType { get; set; } = string.Empty;
    public int Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
