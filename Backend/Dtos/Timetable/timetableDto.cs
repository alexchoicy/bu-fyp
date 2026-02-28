

using System.Text.Json.Serialization;
using Backend.Dtos.Courses;
using Backend.Models;

public class TimetableGenerationRequestDto
{
    public required TimetableScoring Scoring { get; set; }
    public TimetableFilterDto Filter { get; set; } = new();
}


public class TimetableFilterDto
{
    public TimeOnly? EarliestStart { get; set; }
    public TimeOnly? LatestEnd { get; set; }
    public List<TimetableNoClassTimeDto> NoClassTime { get; set; } = new();
}

public class TimetableNoClassTimeDto
{
    public int Day { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}

public class TimetableSuggestionLayoutDto
{
    public TimetableSectionDto[] Sections { get; set; } = Array.Empty<TimetableSectionDto>();
    public double FinalScore { get; set; } = 0.0;
    public List<string> ScoreReasons { get; set; } = new();
}

public class TimetableSuggestionsResponseDto
{
    public List<TimetableSuggestionLayoutDto> RecommendedLayouts { get; set; } = new();
    public TimetableDemoBadLayoutDto? DemoBadLayout { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class TimetableDemoBadLayoutDto
{
    public TimetableSectionDto[] Sections { get; set; } = Array.Empty<TimetableSectionDto>();
    public double FinalScore { get; set; } = 0.0;
    public List<string> ScoreReasons { get; set; } = new();
}

public class TimetableScoring
{
    public int BaseScore { get; set; } // filter out schedules below this score

    public required TimetableScoringGroupWeight GroupWeights { get; set; }
    public required TimetableScoringScheduleShape ScheduleShape { get; set; }
    public required TimetablePreferenceShape PreferenceShape { get; set; }
    public required TimetableGapCompactnessShape GapCompactnessShape { get; set; }
    public required TimetableAssessmentShape AssessmentShape { get; set; }
}

public class TimetableScoringGroupWeight
{
    public double schedule { get; set; } = 0.4;
    public double timePreference { get; set; } = 0.25;
    public double gap { get; set; } = 0.2;
    public double assessments { get; set; } = 0.15;
}

// each scoring aspect uses one point value for both reward and penalty sides
public class TimetableScoringScheduleShape
{
    public required TimetableScoringFreeDayScore FreeDayScore { get; set; }
    public required TimetableScoringSingleClassDayScore SingleClassDayScore { get; set; }
    public required TimetableScoringLongDayScore LongDayScore { get; set; }
    public required TimetableDailyLoadScore DailyLoadScore { get; set; }
}

public class TimetableScoringFreeDayScore
{
    public double Points { get; set; } = 3.0;
}

public class TimetableScoringSingleClassDayScore
{
    public double Points { get; set; } = 3.0;
}

public class TimetableScoringLongDayScore
{
    public double Points { get; set; } = 3.0;
    public double MaxMinutesPerDay { get; set; }
}

public class TimetableDailyLoadScore
{
    public double Points { get; set; } = 3.0;
    public double IdealActiveDays { get; set; }
}

public class TimetablePreferenceShape
{
    public required StartTimePreference StartTimePreference { get; set; }
    public required EndTimePreference EndTimePreference { get; set; }
}

public class StartTimePreference
{
    public TimeOnly PreferredStartTime { get; set; }
    public double Points { get; set; } = 3.0;
}

public class EndTimePreference
{
    public TimeOnly PreferredEndTime { get; set; }
    public double Points { get; set; } = 3.0;
}

public class TimetableGapCompactnessShape
{
    public required CompactGap ShortGap { get; set; }
}


public class CompactGap
{
    public double Points { get; set; } = 3.0;
    public double MaxGapMinutes { get; set; }
    public TimeOnly IgnoreGapStartTime { get; set; }
    public TimeOnly IgnoreGapEndTime { get; set; }
}

public class TimetableAssessmentShape
{
    public List<TimeTableAssessmentScoring> AssessmentCategoryScores { get; set; } = new List<TimeTableAssessmentScoring>();
}

public class TimeTableAssessmentScoring
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AssessmentCategory Category { get; set; }
    public double Points { get; set; } = 3.0;
}
