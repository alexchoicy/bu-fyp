

public class TimetableGenerationRequestDto
{

}


public class TimetableScoring
{
    public int BaseScore { get; set; } // filter out schedules below this score

    public required TimetableScoringGroupWeight GroupWeights { get; set; }
    public required TimetableScoringScheduleShape ScheduleShape { get; set; }


}

public class TimetableScoringGroupWeight
{
    public double schedule { get; set; } = 0.4;
    public double timePreference { get; set; } = 0.25;
    public double gap { get; set; } = 0.2;
    public double assessments { get; set; } = 0.15;
}

// the idea is each scoring aspect has a max points and a max penalty
// user will set when the penalty(maxPenalty), how much points to penalty
// how much point to reward(maxPoints)
public class TimetableScoringScheduleShape
{
    public required TimetableScoringFreeDayScore FreeDayScore { get; set; }
    public required TimetableScoringSingleClassDayScore SingleClassDayScore { get; set; }
    public required TimetableScoringLongDayScore LongDayScore { get; set; }
    public required TimetableDailyLoadScore DailyLoadScore { get; set; }
}

public class TimetableScoringFreeDayScore
{
    public double RewardPoints { get; set; }
    public double PenaltyPoints { get; set; }
}

public class TimetableScoringSingleClassDayScore
{
    public double RewardPoints { get; set; }
    public double PenaltyPoints { get; set; }
}

public class TimetableScoringLongDayScore
{
    public double RewardPoints { get; set; }
    public double PenaltyPoints { get; set; }
    public double MaxMinutesPerDay { get; set; }
}

public class TimetableDailyLoadScore
{
    public double RewardPoints { get; set; }
    public double PenaltyPoints { get; set; }
    public double IdealActiveDays { get; set; }
}