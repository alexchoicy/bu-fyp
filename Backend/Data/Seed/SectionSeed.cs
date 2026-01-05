using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

//Old section seed not used now
public class SectionSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedSectionsAndMeetingsAsync(context);
    }

    private static async Task SeedSectionsAndMeetingsAsync(AppDbContext context)
    {
        if (await context.CourseSections.AnyAsync())
            return;

        // Get required dependencies
        var semester1 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 1");
        var semester2 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 2");
        var courseVersions = await context.CourseVersions.Include(cv => cv.Course).ToListAsync();

        if (!courseVersions.Any() || semester1 == null || semester2 == null)
        {
            Console.WriteLine("Error: Required dependencies not found. Please seed CourseVersions and Terms first.");
            return;
        }

        var sections = new List<CourseSection>();

        // Create sections for all course versions
        // 2 sections (A and B) for each course version
        foreach (var version in courseVersions)
        {
            sections.Add(new CourseSection
            {
                CourseVersionId = version.Id,
                Year = version.FromYear,
                TermId = version.FromTermId,
                SectionNumber = 1 // Section A
            });

            sections.Add(new CourseSection
            {
                CourseVersionId = version.Id,
                Year = version.FromYear,
                TermId = version.FromTermId,
                SectionNumber = 2 // Section B
            });
        }

        await context.CourseSections.AddRangeAsync(sections);
        await context.SaveChangesAsync();

        var createdSections = await context.CourseSections.Include(s => s.CourseVersion).ThenInclude(cv => cv.Course).ToListAsync();

        var meetings = new List<CourseMeeting>();

        foreach (var section in createdSections)
        {
            var courseNumber = section.CourseVersion?.Course?.CourseNumber;

            if (courseNumber == "4117")
            {
                meetings.AddRange(new[]
                {
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 1, // Monday
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 3, // Wednesday
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 5, // Friday
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(10, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Tutorial",
                        Day = 2, // Tuesday
                        StartTime = new TimeOnly(14, 0),
                        EndTime = new TimeOnly(15, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lab",
                        Day = 4, // Thursday
                        StartTime = new TimeOnly(14, 0),
                        EndTime = new TimeOnly(16, 0)
                    }
                });
            }
            // Course 1 (COMP2016): Mon/Wed/Fri 10:45-12:15, Tue 15:45-17:15, Thu 15:30-17:30
            // This creates a conflict with COMP4117 on Mon/Wed/Fri (overlaps 10:45-10:30)
            else if (courseNumber == "2016")
            {
                meetings.AddRange(new[]
                {
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 1, // Monday
                        StartTime = new TimeOnly(10, 45),
                        EndTime = new TimeOnly(12, 15)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 3, // Wednesday
                        StartTime = new TimeOnly(10, 45),
                        EndTime = new TimeOnly(12, 15)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 5, // Friday
                        StartTime = new TimeOnly(10, 45),
                        EndTime = new TimeOnly(12, 15)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Tutorial",
                        Day = 2, // Tuesday
                        StartTime = new TimeOnly(15, 45),
                        EndTime = new TimeOnly(17, 15)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lab",
                        Day = 4, // Thursday
                        StartTime = new TimeOnly(15, 30),
                        EndTime = new TimeOnly(17, 30)
                    }
                });
            }
            // Course 2 (MATH2005): Mon/Wed/Fri 13:00-14:30, Tue 11:00-12:30, Thu 11:00-13:00
            else if (courseNumber == "2005")
            {
                meetings.AddRange(new[]
                {
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 1, // Monday
                        StartTime = new TimeOnly(13, 0),
                        EndTime = new TimeOnly(14, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 3, // Wednesday
                        StartTime = new TimeOnly(13, 0),
                        EndTime = new TimeOnly(14, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lecture",
                        Day = 5, // Friday
                        StartTime = new TimeOnly(13, 0),
                        EndTime = new TimeOnly(14, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Tutorial",
                        Day = 2, // Tuesday
                        StartTime = new TimeOnly(11, 0),
                        EndTime = new TimeOnly(12, 30)
                    },
                    new CourseMeeting
                    {
                        SectionId = section.Id,
                        MeetingType = "Lab",
                        Day = 4, // Thursday
                        StartTime = new TimeOnly(11, 0),
                        EndTime = new TimeOnly(13, 0)
                    }
                });
            }
        }

        await context.CourseMeetings.AddRangeAsync(meetings);
        await context.SaveChangesAsync();
    }
}

