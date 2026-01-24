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


        public static double ComputeFinalScore(TimeTableLayout layout, double weightQuality = 1)
        {
            if (layout == null) return 0.0;

            var quality = ComputeScheduleQuality(layout);

            var final = weightQuality * quality;
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
    }
}
