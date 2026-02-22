using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Services.Timetable
{
    public static class TimetableLayoutFilter
    {
        public static bool Filter(TimeTableLayout layout, TimetableFilterDto filter)
        {
            var meetings = layout.Sections
                .SelectMany(section => section.CourseMeetings)
                .ToList();

            if (meetings.Count == 0)
            {
                return true;
            }


            if (filter.EarliestStart.HasValue && meetings.Any(meeting => meeting.StartTime < filter.EarliestStart))
            {
                return false;
            }

            if (filter.LatestEnd.HasValue && meetings.Any(meeting => meeting.EndTime > filter.LatestEnd))
            {
                return false;
            }

            if (filter.NoClassTime.Count == 0)
            {
                return true;
            }

            foreach (var blocked in filter.NoClassTime)
            {
                var hasConflict = meetings.Any(meeting =>
                    meeting.Day == blocked.Day &&
                    meeting.StartTime < blocked.End &&
                    blocked.Start < meeting.EndTime);

                if (hasConflict)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
