using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Models;

namespace Backend.Services.Timetable
{
    public static class TimetableLayoutFilter
    {
        public static List<TimeTableLayout> FilterLayoutsWithRequirements(List<TimeTableLayout> layouts, int minGapMinutes, int maxGapMinutes, int maxMeetingsPerDay)
        {
            if (layouts == null)
            {
                return new List<TimeTableLayout>();
            }

            for (int i = layouts.Count - 1; i >= 0; i--)
            {
                var layout = layouts[i];
                var dayMeetings = new Dictionary<int, List<(TimeOnly Start, TimeOnly End)>>();

                foreach (var section in layout.Sections)
                {
                    foreach (var meeting in section.CourseMeetings)
                    {
                        if (!dayMeetings.ContainsKey(meeting.Day))
                        {
                            dayMeetings[meeting.Day] = new List<(TimeOnly Start, TimeOnly End)>();
                        }
                        dayMeetings[meeting.Day].Add((meeting.StartTime, meeting.EndTime));
                    }
                }

                if (dayMeetings.Any(dm => dm.Value.Count > maxMeetingsPerDay))
                {
                    layouts.RemoveAt(i);
                    continue;
                }

                bool hasInvalidGap = false;
                foreach (var day in dayMeetings.Keys)
                {
                    var meetings = dayMeetings[day];
                    meetings.Sort((a, b) => a.Start.CompareTo(b.Start));

                    for (int j = 1; j < meetings.Count; j++)
                    {
                        var gap = meetings[j].Start - meetings[j - 1].End;
                        if (gap.TotalMinutes < minGapMinutes)
                        {
                            hasInvalidGap = true;
                            break;
                        }
                        if (gap.TotalMinutes > maxGapMinutes)
                        {
                            hasInvalidGap = true;
                            break;
                        }
                    }

                    if (hasInvalidGap)
                    {
                        break;
                    }
                }

                if (hasInvalidGap)
                {
                    layouts.RemoveAt(i);
                }
            }

            return layouts;
        }
    }
}
