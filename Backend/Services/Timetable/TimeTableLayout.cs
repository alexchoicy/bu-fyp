using Backend.Models;

namespace Backend.Services.Timetable;

public class TimeTableLayout
{
    public List<CourseSection> Sections { get; set; } = new();
    public int rank { get; set; } = 0;
}
