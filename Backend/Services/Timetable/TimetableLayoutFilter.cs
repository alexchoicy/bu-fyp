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
            var blockedTimes = filter.NoClassTime ?? new List<TimetableNoClassTimeDto>();

            if (blockedTimes.Count == 0)
            {
                return true;
            }

            foreach (var blocked in blockedTimes)
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
