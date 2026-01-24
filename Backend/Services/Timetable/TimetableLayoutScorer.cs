using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Models;

namespace Backend.Services.Timetable
{
    public static class TimetableLayoutScorer
    {
        private const double BaseQuality = 50.0;
        private const double SingleClassDayPenalty = 20.0;
        private const double FreeDayReward = 15.0;
        private const double LongDayThresholdMinutes = 8 * 60; // 480 minutes
        private const double LongDayPenaltyPerExtraMinute = 0.05; // penalty per minute beyond threshold

        // Workload scoring defaults
        private static readonly IReadOnlyDictionary<AssessmentCategory, int> DefaultCategoryScore = new Dictionary<AssessmentCategory, int>
        {
            { AssessmentCategory.Examination, 5 },
            { AssessmentCategory.Assignment, 4 },
            { AssessmentCategory.SoloProject, 5 },
            { AssessmentCategory.Participation, 4 },
            { AssessmentCategory.Project, 3 },
            { AssessmentCategory.GroupProject, 2 },
            { AssessmentCategory.Presentation, 2 },
            { AssessmentCategory.Other, 1 },
        };

        private const double CourseWorkloadThreshold = 0;
        private const double WorkloadPenaltyPerUnit = 8.0;

        public static double ComputeFinalScore(TimeTableLayout layout, double weightQuality = 1)
        {
            if (layout == null) return 0.0;

            var quality = ComputeScheduleQuality(layout);

            var workloadPenalty = ComputeAssessmentWorkloadPenalty(layout);

            var final = weightQuality * quality + workloadPenalty;
            return final;
        }

        public static double ComputeScheduleQuality(TimeTableLayout layout)
        {
            if (layout == null) return 0.0;

            // monday
            const int firstDay = 1;
            const int numDays = 5;
            var days = Enumerable.Range(firstDay, numDays).ToList();

            var meetingsByDay = GetMeetingsByDay(layout);

            double score = BaseQuality;

            foreach (var d in days)
            {
                meetingsByDay.TryGetValue(d, out var meetings);
                meetings = meetings ?? new List<CourseMeeting>();

                if (meetings.Count == 0)
                {
                    score += FreeDayReward;
                    layout.ScoreReasons.Add($"Free day on day {d} (+{FreeDayReward})");
                }
                else if (meetings.Count == 1)
                {
                    score -= SingleClassDayPenalty;
                    layout.ScoreReasons.Add($"Single class day on day {d} (-{SingleClassDayPenalty})");
                }

                var totalMinutes = meetings.Sum(m => (m.EndTime - m.StartTime).TotalMinutes);
                if (totalMinutes > LongDayThresholdMinutes)
                {
                    var extra = totalMinutes - LongDayThresholdMinutes;
                    score -= extra * LongDayPenaltyPerExtraMinute;
                    layout.ScoreReasons.Add($"Long day on day {d} (-{extra * LongDayPenaltyPerExtraMinute})");
                }
            }

            return score;
        }

        private static Dictionary<int, List<CourseMeeting>> GetMeetingsByDay(TimeTableLayout layout)
        {
            var dict = new Dictionary<int, List<CourseMeeting>>();

            foreach (var section in layout.Sections)
            {
                if (section.CourseMeetings == null) continue;

                foreach (var m in section.CourseMeetings)
                {
                    if (!dict.ContainsKey(m.Day))
                        dict[m.Day] = new List<CourseMeeting>();

                    dict[m.Day].Add(m);
                }
            }

            return dict;
        }

        private static double ComputeCourseWorkload(IEnumerable<CourseAssessment>? assessments)
        {
            if (assessments == null) return 0.0;
            double total = 0.0;
            foreach (var a in assessments)
            {
                var categoryScore = DefaultCategoryScore.TryGetValue(a.Category, out var s) ? s : 1;
                var normalizedCategory = categoryScore / 5.0; // maps 1-5 -> 0.2..1.0
                var normalizedWeight = (double)a.Weighting / 100.0; // 0..1
                total += normalizedCategory * normalizedWeight;
            }
            return total;
        }

        private static double ComputeAssessmentWorkloadPenalty(TimeTableLayout layout)
        {
            if (layout == null) return 0.0;

            double totalPenalty = 0.0;

            var distinctCourseVersions = layout.Sections
                .Where(s => s.CourseVersion != null)
                .GroupBy(s => s.CourseVersion.Id)
                .Select(g => g.First().CourseVersion)
                .ToList();

            foreach (var cv in distinctCourseVersions)
            {
                var courseWorkload = ComputeCourseWorkload(cv.Assessments);

                var penalty = (courseWorkload - CourseWorkloadThreshold) * WorkloadPenaltyPerUnit;
                totalPenalty += penalty;
                layout.ScoreReasons.Add($"High assessment load for course version {cv.Course.Code.Name}{cv.Course.CourseNumber} (-{penalty:F1})");
            }

            return totalPenalty;
        }
    }
}
