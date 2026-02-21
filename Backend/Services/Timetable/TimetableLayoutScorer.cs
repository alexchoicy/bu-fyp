using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Models;

namespace Backend.Services.Timetable
{
    public static class TimetableLayoutScorer
    {
        private const double MaxRewardPoints = 100.0;
        private const double MaxPenaltyPoints = 90.0;

        public static double ScoreTimetableLayout(TimeTableLayout layout, TimetableScoring scoring)
        {
            double scheduleScore = ScoreScheduleShape(layout, scoring.ScheduleShape);
            double timePreferenceScore = ScoreTimePreference(layout, scoring.PreferenceShape);
            double gapScore = ScoreGapCompactness(layout, scoring.GapCompactnessShape);
            double assessmentScore = ScoreAssessments(layout, scoring.AssessmentShape);

            layout.ScheduleQuality = scheduleScore;
            layout.ScoreReasons.Clear();

            double totalWeight =
                Math.Max(scoring.GroupWeights.schedule, 0) +
                Math.Max(scoring.GroupWeights.timePreference, 0) +
                Math.Max(scoring.GroupWeights.gap, 0) +
                Math.Max(scoring.GroupWeights.assessments, 0);

            if (totalWeight <= 0)
            {
                layout.FinalScore = 0;
                layout.ScoreReasons.Add("Weights sum to 0; final score set to 0.");
                return 0;
            }

            double normalizedScheduleWeight = Math.Max(scoring.GroupWeights.schedule, 0) / totalWeight;
            double normalizedTimePreferenceWeight = Math.Max(scoring.GroupWeights.timePreference, 0) / totalWeight;
            double normalizedGapWeight = Math.Max(scoring.GroupWeights.gap, 0) / totalWeight;
            double normalizedAssessmentWeight = Math.Max(scoring.GroupWeights.assessments, 0) / totalWeight;

            double totalScore =
                scheduleScore * scoring.GroupWeights.schedule +
                timePreferenceScore * scoring.GroupWeights.timePreference +
                gapScore * scoring.GroupWeights.gap +
                assessmentScore * scoring.GroupWeights.assessments;

            double finalScore = totalScore / totalWeight;
            layout.FinalScore = finalScore;

            layout.ScoreReasons.Add($"Schedule shape score: {scheduleScore:F2} (weight {normalizedScheduleWeight:P0})");
            layout.ScoreReasons.Add($"Time preference score: {timePreferenceScore:F2} (weight {normalizedTimePreferenceWeight:P0})");
            layout.ScoreReasons.Add($"Gap compactness score: {gapScore:F2} (weight {normalizedGapWeight:P0})");
            layout.ScoreReasons.Add($"Assessment score: {assessmentScore:F2} (weight {normalizedAssessmentWeight:P0})");
            layout.ScoreReasons.Add($"Final weighted score: {finalScore:F2}");

            return finalScore;
        }

        private static double ScoreScheduleShape(TimeTableLayout layout, TimetableScoringScheduleShape shape)
        {
            var dayMeetings = BuildDayMeetings(layout);
            int activeDays = dayMeetings.Count;
            int consideredWeekdays = 6;

            // 
            double freeDayRatio = ClampRatio((consideredWeekdays - activeDays) / (double)consideredWeekdays);

            double freeDayScore = ScoreFromRatio(
                shape.FreeDayScore.RewardPoints,
                shape.FreeDayScore.PenaltyPoints,
                freeDayRatio);

            double singleClassRatio = activeDays == 0
                ? 1
                : ClampRatio(dayMeetings.Values.Count(m => m.Count == 1) / (double)activeDays);

            double singleClassDayScore = ScoreFromRatio(
                shape.SingleClassDayScore.RewardPoints,
                shape.SingleClassDayScore.PenaltyPoints,
                singleClassRatio);

            double longDayRatio = 1;
            if (activeDays > 0)
            {
                int compliantDays = dayMeetings.Values.Count(meetings =>
                {
                    var ordered = meetings.OrderBy(m => m.StartTime).ToList();
                    double minutes = (ordered[^1].EndTime - ordered[0].StartTime).TotalMinutes;
                    return minutes <= shape.LongDayScore.MaxMinutesPerDay;
                });

                longDayRatio = ClampRatio(compliantDays / (double)activeDays);
            }

            double longDayScore = ScoreFromRatio(
                shape.LongDayScore.RewardPoints,
                shape.LongDayScore.PenaltyPoints,
                longDayRatio);

            double dailyLoadRatio;
            if (shape.DailyLoadScore.IdealActiveDays <= 0)
            {
                dailyLoadRatio = activeDays == 0 ? 1 : 0;
            }
            else
            {
                // double distance = Math.Abs(activeDays - shape.DailyLoadScore.IdealActiveDays);
                // dailyLoadRatio = ClampRatio(1 - (distance / shape.DailyLoadScore.IdealActiveDays));

                dailyLoadRatio = activeDays <= shape.DailyLoadScore.IdealActiveDays
                    ? 1
                    : ClampRatio(1 - ((activeDays - shape.DailyLoadScore.IdealActiveDays) / shape.DailyLoadScore.IdealActiveDays));
            }

            double dailyLoadScore = ScoreFromRatio(
                shape.DailyLoadScore.RewardPoints,
                shape.DailyLoadScore.PenaltyPoints,
                dailyLoadRatio);

            return Average(new[] { freeDayScore, singleClassDayScore, longDayScore, dailyLoadScore });
        }

        private static double ScoreTimePreference(TimeTableLayout layout, TimetablePreferenceShape preferenceShape)
        {
            var meetings = layout.Sections
                .SelectMany(s => s.CourseMeetings)
                .ToList();

            if (meetings.Count == 0)
            {
                double emptyStart = ScoreFromRatio(
                    preferenceShape.StartTimePreference.RewardPoints,
                    preferenceShape.StartTimePreference.PenaltyPoints,
                    1);

                double emptyEnd = ScoreFromRatio(
                    preferenceShape.EndTimePreference.RewardPoints,
                    preferenceShape.EndTimePreference.PenaltyPoints,
                    1);

                return Average(new[] { emptyStart, emptyEnd });
            }

            double startRatio = ClampRatio(meetings.Count(m => m.StartTime >= preferenceShape.StartTimePreference.PreferredStartTime) / (double)meetings.Count);
            double endRatio = ClampRatio(meetings.Count(m => m.EndTime <= preferenceShape.EndTimePreference.PreferredEndTime) / (double)meetings.Count);

            double startScore = ScoreFromRatio(
                preferenceShape.StartTimePreference.RewardPoints,
                preferenceShape.StartTimePreference.PenaltyPoints,
                startRatio);

            double endScore = ScoreFromRatio(
                preferenceShape.EndTimePreference.RewardPoints,
                preferenceShape.EndTimePreference.PenaltyPoints,
                endRatio);

            return Average(new[] { startScore, endScore });
        }

        private static double ScoreGapCompactness(TimeTableLayout layout, TimetableGapCompactnessShape gapShape)
        {
            var dayMeetings = BuildDayMeetings(layout);

            int consideredGaps = 0;
            int compactGaps = 0;

            foreach (var meetings in dayMeetings.Values)
            {
                var ordered = meetings.OrderBy(m => m.StartTime).ToList();
                for (int i = 1; i < ordered.Count; i++)
                {
                    var previous = ordered[i - 1];
                    var current = ordered[i];

                    if (ShouldIgnoreGap(previous.EndTime, current.StartTime, gapShape.ShortGap.IgnoreGapStartTime, gapShape.ShortGap.IgnoreGapEndTime))
                    {
                        continue;
                    }

                    consideredGaps++;
                    double gapMinutes = (current.StartTime - previous.EndTime).TotalMinutes;
                    if (gapMinutes <= gapShape.ShortGap.MaxGapMinutes)
                    {
                        compactGaps++;
                    }
                }
            }

            double compactnessRatio = consideredGaps == 0 ? 1 : ClampRatio(compactGaps / (double)consideredGaps);

            return ScoreFromRatio(
                gapShape.ShortGap.RewardPoints,
                gapShape.ShortGap.PenaltyPoints,
                compactnessRatio);
        }

        private static double ScoreAssessments(TimeTableLayout layout, TimetableAssessmentShape assessmentShape)
        {
            if (assessmentShape.AssessmentCategoryScores.Count == 0)
            {
                return 0;
            }

            var presentBuckets = layout.Sections
                .SelectMany(s => s.CourseVersion.Assessments)
                .Select(a => ToAssessmentBucket(a.Category))
                .ToHashSet();

            var seenBuckets = new HashSet<AssessmentBucket>();
            var scores = new List<double>();

            foreach (var categoryScore in assessmentShape.AssessmentCategoryScores)
            {
                var bucket = ToAssessmentBucket(categoryScore.Category);
                if (!seenBuckets.Add(bucket))
                {
                    continue;
                }

                bool hasCategory = presentBuckets.Contains(bucket);
                double score = hasCategory
                    ? ClampReward(categoryScore.RewardPoints)
                    : -ClampPenalty(categoryScore.PenaltyPoints);

                scores.Add(score);
            }

            return Average(scores);
        }

        private static Dictionary<int, List<CourseMeeting>> BuildDayMeetings(TimeTableLayout layout)
        {
            return layout.Sections
                .SelectMany(s => s.CourseMeetings)
                .GroupBy(m => m.Day)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static bool ShouldIgnoreGap(TimeOnly gapStart, TimeOnly gapEnd, TimeOnly ignoreStart, TimeOnly ignoreEnd)
        {
            if (ignoreStart == ignoreEnd)
            {
                return false;
            }

            return gapStart < ignoreEnd && gapEnd > ignoreStart;
        }

        private static double ScoreFromRatio(double rewardPoints, double penaltyPoints, double ratio)
        {
            double reward = ClampReward(rewardPoints);
            double penalty = ClampPenalty(penaltyPoints);
            double boundedRatio = ClampRatio(ratio);

            // Ratio higher means better.

            return (reward * boundedRatio) - (penalty * (1 - boundedRatio));
        }

        private static double ClampReward(double value)
        {
            return Math.Clamp(value, 0, MaxRewardPoints);
        }

        private static double ClampPenalty(double value)
        {
            return Math.Clamp(value, 0, MaxPenaltyPoints);
        }

        private static double ClampRatio(double value)
        {
            return Math.Clamp(value, 0, 1);
        }

        private static double Average(IEnumerable<double> values)
        {
            var list = values.ToList();
            return list.Count == 0 ? 0 : list.Average();
        }

        private static AssessmentBucket ToAssessmentBucket(AssessmentCategory category)
        {
            return category switch
            {
                AssessmentCategory.Examination => AssessmentBucket.Examination,
                AssessmentCategory.Assignment => AssessmentBucket.Assignment,
                AssessmentCategory.Project => AssessmentBucket.ProjectOrGroupProject,
                AssessmentCategory.GroupProject => AssessmentBucket.ProjectOrGroupProject,
                AssessmentCategory.SoloProject => AssessmentBucket.SoloProject,
                AssessmentCategory.Participation => AssessmentBucket.Participation,
                AssessmentCategory.Presentation => AssessmentBucket.Presentation,
                _ => AssessmentBucket.Other
            };
        }

        private enum AssessmentBucket
        {
            Examination,
            Assignment,
            ProjectOrGroupProject,
            SoloProject,
            Participation,
            Presentation,
            Other
        }
    }
}
