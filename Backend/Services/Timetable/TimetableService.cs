using Backend.Data;
using Backend.Models;
using Backend.Services.Programmes;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Timetable;

public interface ITimetableService
{
    Task GetSuggestionsTimetableAsync(string userId);
}


public class TimetableService : ITimetableService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TimetableService> _logger;
    private readonly IEvaluateRule _evaluateRule;

    public TimetableService(AppDbContext context, ILogger<TimetableService> logger, IEvaluateRule evaluateRule)
    {
        _context = context;
        _logger = logger;
        _evaluateRule = evaluateRule;
    }

    //year here is entry year
    public async Task<IEnumerable<ProgrammeSuggestedCourseSchedule>> GetUserSuggestedScheduleAsync(string userId, int year, int semester)
    {

        var request = await _context.StudentProgrammes
        .Include(p => p.ProgrammeVersion)
        .ThenInclude(v => v.SuggestedCourseSchedules.Where(s => s.StudyYear == year && s.TermId == semester))
        .FirstOrDefaultAsync(sp => sp.StudentId == userId);

        if (request == null)
        {
            _logger.LogWarning($"No programme found for user {userId} with study year {year}");
            return Enumerable.Empty<ProgrammeSuggestedCourseSchedule>();
        }

        return request.ProgrammeVersion.SuggestedCourseSchedules;
    }

    // I dunno the name umm.. temp maybe can reuse later i dunno
    public async Task<List<CourseSection>> GetMustCourseSections(List<int> courseIds, int year, int semester, List<int> studentPassedCourseVersionsIds, List<string> errors)
    {
        var latestVersionIds = await _context.CourseVersions
            .Where(cv => courseIds.Contains(cv.CourseId))
            .GroupBy(cv => cv.CourseId)
            .Select(g => g.OrderByDescending(cv => cv.VersionNumber).First().Id)
            .ToListAsync();

        var latestVersion = await _context.CourseVersions
            .Include(cv => cv.AntiRequisites)
            .Include(cv => cv.Prerequisites)
            .Where(cv => latestVersionIds.Contains(cv.Id))
            .ToListAsync();

        var versionsToRemove = new HashSet<int>();

        foreach (var version in latestVersion)
        {
            foreach (var prereq in version.Prerequisites)
            {
                if (!studentPassedCourseVersionsIds.Contains(prereq.RequiredCourseVersionId))
                {
                    errors.Add($"Missing prerequisite for course {version.CourseId}");
                    versionsToRemove.Add(version.Id);
                    break;
                }
            }

            if (versionsToRemove.Contains(version.Id))
                continue;

            foreach (var antireq in version.AntiRequisites)
            {
                if (studentPassedCourseVersionsIds.Contains(antireq.ExcludedCourseVersionId))
                {
                    errors.Add($"Has taken anti-requisite for course {version.CourseId}");
                    versionsToRemove.Add(version.Id);
                    break;
                }
            }
        }

        var validVersionIds = latestVersion.Select(v => v.Id).Except(versionsToRemove).ToList();

        var sections = await _context.CourseSections
            .Include(cs => cs.CourseMeetings)
            .Include(cs => cs.CourseVersion)
            .ThenInclude(cv => cv.Course)
            .Where(cs => validVersionIds.Contains(cs.CourseVersionId) &&
                cs.TermId == semester && cs.Year == year)
            .ToListAsync();

        return sections;
    }


    private bool HasTimeConflict(List<CourseSection> existingSections, CourseSection candidate)
    {
        foreach (var existing in existingSections)
        {
            foreach (var existingMeeting in existing.CourseMeetings)
            {
                foreach (var candidateMeeting in candidate.CourseMeetings)
                {
                    if (existingMeeting.Day != candidateMeeting.Day)
                    {
                        continue;
                    }

                    var overlap =
                        candidateMeeting.StartTime < existingMeeting.EndTime &&
                        existingMeeting.StartTime < candidateMeeting.EndTime;

                    if (overlap)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public List<TimeTableLayout> GetAllPossibleTimetableLayouts(
        List<CourseSection> mustCourseSections,
        Dictionary<int, List<CourseSection>>? mustOptionalCourseSections = null,
        List<CourseSection>? freeElectiveCourseSections = null,
        int freeElectiveCreditsRequired = 0)
    {
        if (mustCourseSections == null || mustCourseSections.Count == 0)
        {
            return new List<TimeTableLayout>();
        }

        var finalLayouts = new List<TimeTableLayout>();

        var mustLayouts = GenerateMustCourseLayouts(mustCourseSections);

        if (mustLayouts.Count == 0)
        {
            return new List<TimeTableLayout>();
        }

        // Console.WriteLine($"Must Layouts Count: {mustLayouts.Count}");
        // DebugPrintLayouts(mustLayouts);
        // Console.WriteLine("----");

        var layoutsWithOptional = new List<TimeTableLayout>();

        if (mustOptionalCourseSections != null && mustOptionalCourseSections.Count > 0)
        {
            foreach (var baseLayout in mustLayouts)
            {
                var expandedLayouts = ExpandLayoutWithOptionalSections(baseLayout, mustOptionalCourseSections);

                // Console.WriteLine($"Layouts with Optional Count for base layout rank {baseLayout.rank}: {expandedLayouts.Count}");
                // DebugPrintLayouts(expandedLayouts);
                // Console.WriteLine("----");

                layoutsWithOptional.AddRange(expandedLayouts);
            }
        }
        else
        {
            layoutsWithOptional = mustLayouts;
        }

        // Console.WriteLine($"after Optional Phase Count: {freeElectiveCourseSections.Count}, {freeElectiveCreditsRequired} ");

        if (freeElectiveCourseSections != null && freeElectiveCourseSections.Count > 0 && freeElectiveCreditsRequired > 0)
        {
            foreach (var baseLayout in layoutsWithOptional)
            {
                var layoutsWithElectives = GenerateFreeElectiveCombinations(baseLayout, freeElectiveCourseSections, freeElectiveCreditsRequired);

                // Console.WriteLine($"Layouts with Electives Count for base layout rank {baseLayout.rank}: {layoutsWithElectives.Count}");
                // DebugPrintLayouts(layoutsWithElectives);
                // Console.WriteLine("----");

                finalLayouts.AddRange(layoutsWithElectives);
            }
        }
        else
        {
            finalLayouts = layoutsWithOptional;
        }

        return finalLayouts;
    }

    private List<TimeTableLayout> GenerateMustCourseLayouts(List<CourseSection> mustCourseSections)
    {
        var sectionsByVersion = mustCourseSections
            .GroupBy(section => section.CourseVersionId)
            .ToList();

        var layouts = new List<TimeTableLayout>();
        var current = new List<CourseSection>();

        void Backtrack(int index)
        {
            if (index == sectionsByVersion.Count)
            {
                layouts.Add(new TimeTableLayout { Sections = current.ToList() });
                return;
            }

            foreach (var section in sectionsByVersion[index])
            {
                if (HasTimeConflict(current, section))
                {
                    continue;
                }

                current.Add(section);
                Backtrack(index + 1);
                current.RemoveAt(current.Count - 1);
            }
        }

        Backtrack(0);
        return layouts;
    }

    private List<TimeTableLayout> ExpandLayoutWithOptionalSections(
        TimeTableLayout baseLayout,
        Dictionary<int, List<CourseSection>> optionalSections)
    {
        var results = new List<TimeTableLayout>();
        var optionalGroups = optionalSections.Values.ToList();
        var current = new List<CourseSection>(baseLayout.Sections);

        void BacktrackOptional(int groupIndex)
        {
            if (groupIndex == optionalGroups.Count)
            {
                results.Add(new TimeTableLayout { Sections = current.ToList() });
                return;
            }

            var group = optionalGroups[groupIndex];
            var addedSection = false;

            // Try to add one section from this group
            foreach (var section in group)
            {
                if (!HasTimeConflict(current, section))
                {
                    current.Add(section);
                    addedSection = true;
                    BacktrackOptional(groupIndex + 1);
                    current.RemoveAt(current.Count - 1);
                }
            }

            // If no section from this group fits, skip this group and continue
            if (!addedSection)
            {
                BacktrackOptional(groupIndex + 1);
            }
        }

        BacktrackOptional(0);

        // If no optional sections could be added, return the base layout
        if (results.Count == 0)
        {
            results.Add(baseLayout);
        }

        return results;
    }

    private List<TimeTableLayout> GenerateFreeElectiveCombinations(
        TimeTableLayout baseLayout,
        List<CourseSection> freeElectives,
        int creditsRequired)
    {
        var results = new List<TimeTableLayout>();
        var baseSections = baseLayout.Sections;
        var usedVersionIds = new HashSet<int>(baseSections.Select(s => s.CourseVersionId));

        var availableElectives = freeElectives
            .Where(e => !usedVersionIds.Contains(e.CourseVersionId) && !HasTimeConflict(baseSections, e))
            .ToList();

        // Console.WriteLine($"Available Electives Count: {availableElectives.Count} | {String.Join(", ", availableElectives.Select(e => e.CourseVersionId))}");

        if (availableElectives.Count == 0)
        {
            results.Add(baseLayout);
            return results;
        }

        var validCombinations = new List<List<CourseSection>>();
        GenerateCombinations(availableElectives, new List<CourseSection>(), 0, creditsRequired, validCombinations);

        // Console.WriteLine($"Valid Combinations Count: {validCombinations.Count}");

        if (validCombinations.Count == 0)
        {
            results.Add(baseLayout);
            return results;
        }

        foreach (var combination in validCombinations)
        {
            bool hasConflict = false;
            foreach (var elective in combination)
            {
                if (HasTimeConflict(baseSections, elective))
                {
                    hasConflict = true;
                    break;
                }
            }

            if (!hasConflict)
            {
                var sections = new List<CourseSection>(baseSections);
                sections.AddRange(combination);
                results.Add(new TimeTableLayout { Sections = sections });
            }
        }

        return results;
    }

    private void GenerateCombinations(
        List<CourseSection> availableElectives,
        List<CourseSection> currentCombination,
        int startIndex,
        int creditsRequired,
        List<List<CourseSection>> validCombinations)
    {
        int currentCredits = currentCombination.Sum(cs => cs.CourseVersion.Course.Credit);

        if (currentCredits >= creditsRequired)
        {
            validCombinations.Add(new List<CourseSection>(currentCombination));
            return;
        }

        for (int i = startIndex; i < availableElectives.Count; i++)
        {
            var elective = availableElectives[i];

            if (!HasTimeConflict(currentCombination, elective))
            {
                currentCombination.Add(elective);
                GenerateCombinations(availableElectives, currentCombination, i + 1, creditsRequired, validCombinations);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
    }

    private void DebugPrintLayouts(List<TimeTableLayout> layouts)
    {
        foreach (var layout in layouts)
        {
            Console.WriteLine($"Possible Timetable Layout: Rank {layout.rank}");
            foreach (var section in layout.Sections)
            {
                Console.WriteLine($"CourseVersionId: {section.CourseVersionId}, SectionNumber: {section.SectionNumber}");
                foreach (var meeting in section.CourseMeetings)
                {
                    Console.WriteLine($"  Meeting - Day: {meeting.Day}, StartTime: {meeting.StartTime}, EndTime: {meeting.EndTime}");
                }
            }
        }
    }

    public List<TimeTableLayout> FiliterLayoutsWithRequirements(List<TimeTableLayout> layouts, int minGapMinutes, int maxGapMinutes, int maxMeetingsPerDay)
    {
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

    public async Task<List<CourseSection>> HandleCoreElectiveCourseSections(int year, int semester, int userProgrammeVersionId, List<int> studentPassedCourseIds, List<string> errors)
    {
        var coreElectiveCategories = await _context.ProgrammeCategories
            .Where(pc => pc.ProgrammeVersionId == userProgrammeVersionId && pc.Category.Type == CategoryType.Core_Elective)
            .Include(pc => pc.Category)
            .ToListAsync();

        var resultSections = new List<CourseSection>();

        foreach (var programmeCategory in coreElectiveCategories)
        {
            var category = programmeCategory.Category;

            var completedGroupIds = await MapPassedCoursesToGroupIds(studentPassedCourseIds, category.Id);

            if (category.Rules == null)
                continue;

            var unfulfilledGroupIds = _evaluateRule.GetUnfulfilledGroupsForCategory(category.Rules, completedGroupIds);

            if (unfulfilledGroupIds.Count == 0)
                continue;

            //It suppose to not be duplicate i think ha
            var sections = await GetCourseSectionsByGroupsIds(unfulfilledGroupIds, studentPassedCourseIds, year, semester);

            resultSections.AddRange(sections);
        }

        return resultSections;
    }

    public async Task<List<CourseSection>> GetCourseSectionsByGroupsIds(List<int> groupIds, List<int> studentPassedCourseIds, int year, int semester)
    {
        var groupCourses = await _context.GroupCourses
            .Include(gc => gc.Course)
            .ThenInclude(c => c.CourseVersions)
            .ThenInclude(c => c.Prerequisites)
            .Include(gc => gc.Course)
            .ThenInclude(c => c.CourseVersions)
            .ThenInclude(c => c.AntiRequisites)
            .Where(gc => groupIds.Contains(gc.GroupId))
            .ToListAsync();

        var courseVersionIds = new HashSet<int>();

        foreach (var gc in groupCourses)
        {
            if (gc.Course != null)
            {
                var latestVersion = gc.Course.CourseVersions
                    .OrderByDescending(cv => cv.VersionNumber)
                    .FirstOrDefault();

                if (latestVersion == null)
                {
                    continue;
                }

                bool hasPrereqIssue = latestVersion.Prerequisites.Any(prereq =>
                    !studentPassedCourseIds.Contains(prereq.RequiredCourseVersionId));

                bool hasAntiReqIssue = latestVersion.AntiRequisites.Any(antireq =>
                    studentPassedCourseIds.Contains(antireq.ExcludedCourseVersionId));

                if (!hasPrereqIssue && !hasAntiReqIssue && !courseVersionIds.Contains(latestVersion.Id) && !studentPassedCourseIds.Contains(latestVersion.Id))
                {
                    courseVersionIds.Add(latestVersion.Id);
                }
            }

            if (gc.CodeId.HasValue)
            {
                var codeCourses = await _context.Courses
                    .Where(c => c.CodeId == gc.CodeId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var courseId in codeCourses)
                {
                    var latestVersion = await _context.CourseVersions
                        .Where(cv => cv.CourseId == courseId)
                        .Include(cv => cv.Prerequisites)
                        .Include(cv => cv.AntiRequisites)
                        .OrderByDescending(cv => cv.VersionNumber)
                        .FirstOrDefaultAsync();

                    if (latestVersion == null)
                    {
                        continue;
                    }

                    bool hasPrereqIssue = latestVersion.Prerequisites.Any(prereq =>
                        !studentPassedCourseIds.Contains(prereq.RequiredCourseVersionId));
                    bool hasAntiReqIssue = latestVersion.AntiRequisites.Any(antireq =>
                        studentPassedCourseIds.Contains(antireq.ExcludedCourseVersionId));

                    if (!hasPrereqIssue && !hasAntiReqIssue && !courseVersionIds.Contains(latestVersion.Id) && !studentPassedCourseIds.Contains(latestVersion.Id))
                    {
                        courseVersionIds.Add(latestVersion.Id);
                    }
                }
            }
        }

        // Console.WriteLine("Course Version Ids to fetch sections for: " + string.Join(", ", courseVersionIds));

        var courseSections = await _context.CourseSections
            .Include(cs => cs.CourseMeetings)
            .Include(cs => cs.CourseVersion)
            .ThenInclude(cv => cv.Course)
            .Where(cs => courseVersionIds.Contains(cs.CourseVersionId) &&
                cs.TermId == semester && cs.Year == year)
            .ToListAsync();

        return courseSections;
    }

    private async Task<HashSet<int>> MapPassedCoursesToGroupIds(List<int> studentPassedCourseIds, int categoryId)
    {
        var completedGroupIds = new HashSet<int>();

        if (studentPassedCourseIds.Count == 0)
            return completedGroupIds;

        var groupCourses = await _context.GroupCourses
            .Where(gc => _context.CategoryGroups
                .Where(cg => cg.CategoryId == categoryId)
                .Select(cg => cg.GroupId)
                .Contains(gc.CourseId))
            .ToListAsync();

        foreach (var studentCourseId in studentPassedCourseIds)
        {
            var matchingGroupCourse = groupCourses.FirstOrDefault(gc =>
                gc.CourseId.HasValue && gc.CourseId.Value == studentCourseId);

            if (matchingGroupCourse != null)
            {
                completedGroupIds.Add(matchingGroupCourse.GroupId);
                continue;
            }
        }

        return completedGroupIds;
    }

    public async Task<(Dictionary<int, List<CourseSection>>, List<CourseSection>, int)> GetOtherFlexibleCourseSections(List<ProgrammeSuggestedCourseSchedule> suggestedCourseSchedules, int year, int semester, int userProgrammeVersionId, List<int> studentPassedCourseIds, List<string> errors)
    {
        // first set of List = a set that required in something, List 2 = items in the set
        // ID = requirement id

        var mustSet = new Dictionary<int, List<CourseSection>>();

        var optionalSections = new List<CourseSection>();

        var freeElectiveCreditsRequired = 0;

        foreach (var schedule in suggestedCourseSchedules)
        {
            //Handled in must courses
            if (schedule.CourseId != null)
            {
                continue;
            }

            if (schedule.IsCoreElective)
            {
                var coreSections = await HandleCoreElectiveCourseSections(year, semester, userProgrammeVersionId, studentPassedCourseIds, errors);
                mustSet[schedule.Id] = coreSections;

                Console.WriteLine($"Core Elective Sections Count for Schedule {schedule.Id}: {coreSections.Count}");

                continue;
            }

            if (schedule.IsFreeElective)
            {
                freeElectiveCreditsRequired += (int)schedule.Credits;
                var freeElectiveGroup = await _context.ProgrammeCategories
                    .Where(pc => pc.ProgrammeVersionId == userProgrammeVersionId && pc.Category.Type == CategoryType.Elective)
                    .Include(pc => pc.Category)
                    .ThenInclude(c => c.CategoryGroups)
                    .SelectMany(pc => pc.Category.CategoryGroups)
                    .FirstOrDefaultAsync();

                if (freeElectiveGroup == null)
                {
                    continue;
                }

                var freeElectiveSectionsList = await GetCourseSectionsByGroupsIds(new List<int> { freeElectiveGroup.GroupId.Value }, studentPassedCourseIds, year, semester);

                Console.WriteLine($"Free Elective Sections Count: {freeElectiveSectionsList.Count}");
                optionalSections.AddRange(freeElectiveSectionsList);
            }
        }

        return (mustSet, optionalSections, freeElectiveCreditsRequired);
    }


    public async Task GetSuggestionsTimetableAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);

        var userProgrammeVersionId = await _context.StudentProgrammes
            .Where(sp => sp.StudentId == userId)
            .Select(sp => sp.ProgrammeVersionId)
            .FirstOrDefaultAsync();

        int studyYear = user!.GetCurrentStudyYear();
        int currentAcademicYear = user.EntryAcedmicYear + studyYear - 1;
        int semester = 2;

        var studentCourses = await _context.StudentCourses
            .Where(sc => sc.StudentId == userId)
            .ToListAsync();

        var studentPassedCoursesIds = studentCourses.Where(sc => GradeUtility.IsPassing(sc.Grade ?? Grade.NA) || sc.Status == StudentCourseStatus.Exemption)
            .Select(sc => sc.CourseId);

        var studentPassedCourseVersionsIds = await _context.CourseVersions
            .Where(cv => studentPassedCoursesIds.Contains(cv.CourseId))
            .GroupBy(cv => cv.CourseId)
            .Select(g => g.OrderByDescending(cv => cv.VersionNumber).First().Id)
            .ToListAsync();

        var schedules = await GetUserSuggestedScheduleAsync(userId, studyYear, semester);

        var error = new List<string>();

        var requiredCourseIds = schedules.Where(s => s.CourseId != null).Select(s => s.CourseId!.Value).ToList();

        var mustCourseSections = await GetMustCourseSections(requiredCourseIds, currentAcademicYear, semester, studentPassedCourseVersionsIds, error);

        var (mustOptionalSections, freeElectiveSections, freeElectiveCreditsRequired) = await GetOtherFlexibleCourseSections(schedules.ToList(), currentAcademicYear, semester, userProgrammeVersionId, studentPassedCourseVersionsIds, error);

        var layouts = GetAllPossibleTimetableLayouts(mustCourseSections, mustOptionalSections, freeElectiveSections, freeElectiveCreditsRequired);


        // FiliterLayoutsWithRequirements(layouts, minGapMinutes: 30, maxGapMinutes: 120, maxMeetingsPerDay: 5);



        Console.WriteLine($"Total Valid Layouts Found: {layouts.Count}");
        DebugPrintLayouts(layouts);
        Console.WriteLine("----");



        return;
    }
}
