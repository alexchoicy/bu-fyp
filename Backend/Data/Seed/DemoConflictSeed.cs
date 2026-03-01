using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public static class DemoConflictSeed
{
    private const string DemoUsername = "student2";
    private const string DemoEmail = "student2@bu.edu";
    private const string DemoPassword = "student2";
    private const string DemoCourseNumber = "2999";
    private const int DemoStudyYear = 4;
    private const int DemoAcademicYear = 2025;

    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager)
    {
        await ResetDemoSeedDataAsync(context);

        var semester2 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 2");
        if (semester2 == null)
        {
            return;
        }

        var user = await EnsureDemoUserAsync(userManager);
        if (user == null)
        {
            return;
        }

        var programmeVersion = await context.ProgrammeVersions
            .OrderByDescending(pv => pv.IsActive)
            .ThenByDescending(pv => pv.StartYear)
            .FirstOrDefaultAsync();

        if (programmeVersion == null)
        {
            return;
        }

        await EnsureStudentProgrammeAsync(context, user.Id, programmeVersion.Id);
        var demoCourseId = await EnsureConflictRequiredCourseAsync(context, semester2.Id);

        await EnsureSuggestedCourseScheduleAsync(
            context,
            programmeVersion.Id,
            DemoStudyYear,
            semester2.Id,
            demoCourseId,
            isCoreElective: false,
            isFreeElective: false,
            credits: null);

        await EnsureSuggestedCourseScheduleAsync(
            context,
            programmeVersion.Id,
            DemoStudyYear,
            semester2.Id,
            courseId: null,
            isCoreElective: true,
            isFreeElective: false,
            credits: 3);

        await EnsureSuggestedCourseScheduleAsync(
            context,
            programmeVersion.Id,
            DemoStudyYear,
            semester2.Id,
            courseId: null,
            isCoreElective: false,
            isFreeElective: true,
            credits: 3);
    }

    private static async Task ResetDemoSeedDataAsync(AppDbContext context)
    {
        var semester2Id = await context.Terms
            .Where(t => t.Name == "Semester 2")
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        var demoUserId = await context.Users
            .Where(u => u.Email == DemoEmail)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var demoCourseId = await context.Courses
            .Where(c => c.CourseNumber == DemoCourseNumber && c.Code.Tag == "COMP")
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrEmpty(demoUserId))
        {
            var studentProgrammes = await context.StudentProgrammes
                .Where(sp => sp.StudentId == demoUserId)
                .ToListAsync();

            if (studentProgrammes.Count > 0)
            {
                var programmeVersionIds = studentProgrammes
                    .Select(sp => sp.ProgrammeVersionId)
                    .Distinct()
                    .ToList();

                if (semester2Id.HasValue)
                {
                    var flexibleDemoSchedules = await context.ProgrammeSuggestedCourseSchedules
                        .Where(s => programmeVersionIds.Contains(s.ProgrammeVersionId)
                                    && s.StudyYear == DemoStudyYear
                                    && s.TermId == semester2Id.Value
                                    && s.CourseId == null
                                    && s.Credits == 3
                                    && (s.IsCoreElective || s.IsFreeElective))
                        .ToListAsync();

                    if (flexibleDemoSchedules.Count > 0)
                    {
                        context.ProgrammeSuggestedCourseSchedules.RemoveRange(flexibleDemoSchedules);
                    }
                }

                context.StudentProgrammes.RemoveRange(studentProgrammes);
            }

            var studentCourses = await context.StudentCourses
                .Where(sc => sc.StudentId == demoUserId)
                .ToListAsync();

            if (studentCourses.Count > 0)
            {
                context.StudentCourses.RemoveRange(studentCourses);
            }
        }

        if (demoCourseId.HasValue)
        {
            var scheduleWithDemoCourse = await context.ProgrammeSuggestedCourseSchedules
                .Where(s => s.CourseId == demoCourseId.Value)
                .ToListAsync();

            if (scheduleWithDemoCourse.Count > 0)
            {
                context.ProgrammeSuggestedCourseSchedules.RemoveRange(scheduleWithDemoCourse);
            }

            var demoCourse = await context.Courses.FirstOrDefaultAsync(c => c.Id == demoCourseId.Value);
            if (demoCourse != null)
            {
                context.Courses.Remove(demoCourse);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task<User?> EnsureDemoUserAsync(UserManager<User> userManager)
    {
        var user = await userManager.FindByEmailAsync(DemoEmail);
        if (user == null)
        {
            user = new User
            {
                UserName = DemoUsername,
                Email = DemoEmail,
                Name = "Student B",
                EmailConfirmed = true,
                EntryAcedmicYear = 2022,
                EntryYear = 1
            };

            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                return null;
            }
        }
        else
        {
            var shouldUpdate = user.UserName != DemoUsername
                               || user.Name != "Student B"
                               || user.EntryAcedmicYear != 2022
                               || user.EntryYear != 1;

            if (shouldUpdate)
            {
                user.UserName = DemoUsername;
                user.Name = "Student B";
                user.EntryAcedmicYear = 2022;
                user.EntryYear = 1;

                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return null;
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, Roles.Student.ToString()))
        {
            await userManager.AddToRoleAsync(user, Roles.Student.ToString());
        }

        return user;
    }

    private static async Task EnsureStudentProgrammeAsync(AppDbContext context, string userId, int programmeVersionId)
    {
        var hasLink = await context.StudentProgrammes
            .AnyAsync(sp => sp.StudentId == userId && sp.ProgrammeVersionId == programmeVersionId);

        if (hasLink)
        {
            return;
        }

        await context.StudentProgrammes.AddAsync(new StudentProgramme
        {
            StudentId = userId,
            ProgrammeVersionId = programmeVersionId
        });

        await context.SaveChangesAsync();
    }

    private static async Task<int> EnsureConflictRequiredCourseAsync(AppDbContext context, int semester2Id)
    {
        var course = await context.Courses
            .Include(c => c.CourseVersions)
            .FirstOrDefaultAsync(c => c.CourseNumber == DemoCourseNumber && c.Code.Tag == "COMP");

        if (course == null)
        {
            var compCode = await context.Codes.FirstAsync(c => c.Tag == "COMP");
            course = new Course
            {
                Name = "Demo Timetable Conflict Anchor",
                CourseNumber = DemoCourseNumber,
                CodeId = compCode.Id,
                Credit = 3,
                Description = "Seed-only demo course used to trigger required section conflict responses."
            };

            await context.Courses.AddAsync(course);
            await context.SaveChangesAsync();
        }

        var latestVersion = await context.CourseVersions
            .Where(cv => cv.CourseId == course.Id)
            .OrderByDescending(cv => cv.VersionNumber)
            .FirstOrDefaultAsync();

        if (latestVersion == null)
        {
            latestVersion = new CourseVersion
            {
                CourseId = course.Id,
                VersionNumber = 1,
                Description = "",
                AimAndObjectives = "Demo course for conflict handling.",
                CourseContent = "Always overlaps with core-elective options.",
                CILOs =
                [
                    new CILOs { code = "CILO1", Description = "Trigger timetable conflict path." }
                ],
                TLAs =
                [
                    new TLAs { code = ["Lecture"], Description = "Seed-only timetable conflict demo." }
                ],
                FromYear = DemoAcademicYear,
                FromTermId = semester2Id
            };

            await context.CourseVersions.AddAsync(latestVersion);
            await context.SaveChangesAsync();
        }

        var hasSection = await context.CourseSections
            .AnyAsync(cs => cs.CourseVersionId == latestVersion.Id && cs.Year == DemoAcademicYear && cs.TermId == semester2Id);

        if (!hasSection)
        {
            var section = new CourseSection
            {
                CourseVersionId = latestVersion.Id,
                Year = DemoAcademicYear,
                TermId = semester2Id,
                SectionNumber = 1,
                CourseMeetings =
                [
                    new CourseMeeting { Day = 1, MeetingType = "Lecture", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) },
                    new CourseMeeting { Day = 2, MeetingType = "Lecture", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) },
                    new CourseMeeting { Day = 3, MeetingType = "Lecture", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) },
                    new CourseMeeting { Day = 4, MeetingType = "Lecture", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) },
                    new CourseMeeting { Day = 5, MeetingType = "Lecture", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0) }
                ]
            };

            await context.CourseSections.AddAsync(section);
            await context.SaveChangesAsync();
        }

        return course.Id;
    }

    private static async Task EnsureSuggestedCourseScheduleAsync(
        AppDbContext context,
        int programmeVersionId,
        int studyYear,
        int termId,
        int? courseId,
        bool isCoreElective,
        bool isFreeElective,
        decimal? credits)
    {
        var existingQuery = context.ProgrammeSuggestedCourseSchedules
            .Where(s => s.ProgrammeVersionId == programmeVersionId
                        && s.StudyYear == studyYear
                        && s.TermId == termId);

        if (courseId.HasValue)
        {
            existingQuery = existingQuery.Where(s => s.CourseId == courseId.Value);
        }
        else
        {
            existingQuery = existingQuery.Where(s => !s.CourseId.HasValue
                                                     && s.IsCoreElective == isCoreElective
                                                     && s.IsFreeElective == isFreeElective);
        }

        var existing = await existingQuery.FirstOrDefaultAsync();

        if (existing == null)
        {
            await context.ProgrammeSuggestedCourseSchedules.AddAsync(new ProgrammeSuggestedCourseSchedule
            {
                ProgrammeVersionId = programmeVersionId,
                StudyYear = studyYear,
                TermId = termId,
                CourseId = courseId,
                IsCoreElective = isCoreElective,
                IsFreeElective = isFreeElective,
                Credits = credits
            });

            await context.SaveChangesAsync();
            return;
        }

        var shouldUpdate = existing.IsCoreElective != isCoreElective ||
                           existing.IsFreeElective != isFreeElective ||
                           existing.Credits != credits;

        if (!shouldUpdate)
        {
            return;
        }

        existing.IsCoreElective = isCoreElective;
        existing.IsFreeElective = isFreeElective;
        existing.Credits = credits;
        await context.SaveChangesAsync();
    }
}
