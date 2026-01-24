using Backend.Models;

namespace Backend.Services.Timetable;

public class TimeTableLayout
{
    public List<CourseSection> Sections { get; set; } = new();

    public double ScheduleQuality { get; set; } = 0.0;
    public double FinalScore { get; set; } = 0.0;

    public List<string> ScoreReasons { get; set; } = new();
}
