public class TimetableChatGenerationRequestDto
{
    public TimetableChatFilterDto Filter { get; set; } = new();
    public TimetableChatScoringDto Scoring { get; set; } = new();
}


public class TimetableChatFilterDto
{
    public List<TimetableChatNoClassTimeDto> NoClassTime { get; set; } = new();
}

public class TimetableChatNoClassTimeDto
{
    public int Day { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
}

public class TimetableChatScoringDto
{
    public int BaseScore { get; set; }
    public TimetableChatGroupWeightDto GroupWeights { get; set; } = new();
    public TimetableChatScheduleShapeDto ScheduleShape { get; set; } = new();
    public TimetableChatPreferenceShapeDto PreferenceShape { get; set; } = new();
    public TimetableChatGapCompactnessShapeDto GapCompactnessShape { get; set; } = new();
    public TimetableChatAssessmentShapeDto AssessmentShape { get; set; } = new();
}

public class TimetableChatGroupWeightDto
{
    public double Schedule { get; set; }
    public double TimePreference { get; set; }
    public double Gap { get; set; }
    public double Assessments { get; set; }
}

public class TimetableChatScheduleShapeDto
{
    public TimetableChatPointsDto FreeDayScore { get; set; } = new();
    public TimetableChatPointsDto SingleClassDayScore { get; set; } = new();
    public TimetableChatLongDayScoreDto LongDayScore { get; set; } = new();
    public TimetableChatDailyLoadScoreDto DailyLoadScore { get; set; } = new();
}


public class TimetableChatPointsDto
{
    public double Points { get; set; }
}

public class TimetableChatLongDayScoreDto : TimetableChatPointsDto
{
    public double MaxMinutesPerDay { get; set; }
}

public class TimetableChatDailyLoadScoreDto : TimetableChatPointsDto
{
    public double IdealActiveDays { get; set; }
}

public class TimetableChatPreferenceShapeDto
{
    public TimetableChatStartTimePreferenceDto StartTimePreference { get; set; } = new();
    public TimetableChatEndTimePreferenceDto EndTimePreference { get; set; } = new();
}

public class TimetableChatStartTimePreferenceDto
{
    public string? PreferredStartTime { get; set; }
    public double Points { get; set; }
}

public class TimetableChatEndTimePreferenceDto
{
    public string? PreferredEndTime { get; set; }
    public double Points { get; set; }
}

public class TimetableChatGapCompactnessShapeDto
{
    public TimetableChatCompactGapDto ShortGap { get; set; } = new();
}

public class TimetableChatCompactGapDto : TimetableChatPointsDto
{
    public double MaxGapMinutes { get; set; }
    public string? IgnoreGapStartTime { get; set; }
    public string? IgnoreGapEndTime { get; set; }
}

public class TimetableChatAssessmentShapeDto
{
    public List<TimetableChatAssessmentScoreDto> AssessmentCategoryScores { get; set; } = new();
}

public class TimetableChatAssessmentScoreDto
{
    public string Category { get; set; } = string.Empty;
    public double Points { get; set; }
}