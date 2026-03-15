using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public static class DashboardSeed
{
    private const string DashboardStudentEmail = "student@bu.edu";
    private const string DashboardCourseCode = "COMP";
    private const string DashboardCourseNumber = "3836";
    private const string DashboardCourseName = "Dashboard Demo Course";
    private const int DashboardAcademicYear = 2024;

    public static async Task SeedAsync(AppDbContext context, UserManager<User> userManager)
    {
        var semester2 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 2");
        var compCode = await context.Codes.FirstOrDefaultAsync(c => c.Tag == DashboardCourseCode);
        var student = await userManager.FindByEmailAsync(DashboardStudentEmail);

        if (semester2 == null || compCode == null || student == null)
        {
            return;
        }

        var course = await EnsureDashboardCourseAsync(context, compCode, semester2);
        await EnsureDashboardStudentCourseAsync(context, student, course, semester2);
    }

    private static async Task<Course> EnsureDashboardCourseAsync(AppDbContext context, Code compCode, Term semester2)
    {
        var course = await context.Courses
            .Include(c => c.CourseVersions)
            .FirstOrDefaultAsync(c => c.CodeId == compCode.Id && c.CourseNumber == DashboardCourseNumber);

        if (course == null)
        {
            course = new Course
            {
                Name = DashboardCourseName,
                CourseNumber = DashboardCourseNumber,
                CodeId = compCode.Id,
                Credit = 3,
                Description = "Dashboard demo course for academic progress visualisation."
            };

            await context.Courses.AddAsync(course);
        }
        else
        {
            course.Name = DashboardCourseName;
            course.Credit = 3;
            course.Description = "Dashboard demo course for academic progress visualisation.";
        }

        var version = course.CourseVersions.FirstOrDefault(v => v.VersionNumber == 1);
        if (version == null)
        {
            version = new CourseVersion
            {
                Course = course,
                VersionNumber = 1,
                Description = "A COMP version of the seeded data mining course used to enrich dashboard analytics.",
                AimAndObjectives = "This demo course mirrors the seeded data mining example so the dashboard has meaningful cross-subject GPA data.",
                CourseContent = "Topics include data preparation, classification, clustering, recommendation systems, and communicating insights from mined data.",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Explain core data mining principles and workflows." },
                    new() { code = "CILO2", Description = "Apply machine learning techniques to practical datasets." },
                    new() { code = "CILO3", Description = "Interpret mined results and communicate findings clearly." }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture" }, Description = "Lectures cover data mining concepts and their applications." },
                    new() { code = new[] { "Laboratory" }, Description = "Labs provide hands-on practice with data analysis and modelling." },
                    new() { code = new[] { "Project" }, Description = "Students build a small data mining project to demonstrate understanding." }
                },
                FromYear = DashboardAcademicYear,
                FromTermId = semester2.Id
            };

            await context.CourseVersions.AddAsync(version);
        }
        else
        {
            version.Description = "A COMP version of the seeded data mining course used to enrich dashboard analytics.";
            version.AimAndObjectives = "This demo course mirrors the seeded data mining example so the dashboard has meaningful cross-subject GPA data.";
            version.CourseContent = "Topics include data preparation, classification, clustering, recommendation systems, and communicating insights from mined data.";
            version.FromYear = DashboardAcademicYear;
            version.FromTermId = semester2.Id;
        }

        var hasAssessments = await context.CourseAssessments.AnyAsync(a => a.CourseVersionId == version.Id);
        if (!hasAssessments)
        {
            await context.CourseAssessments.AddRangeAsync(
                new CourseAssessment
                {
                    CourseVersion = version,
                    Name = "Tests",
                    Weighting = 40,
                    Category = AssessmentCategory.Examination
                },
                new CourseAssessment
                {
                    CourseVersion = version,
                    Name = "Project",
                    Weighting = 35,
                    Category = AssessmentCategory.Assignment
                },
                new CourseAssessment
                {
                    CourseVersion = version,
                    Name = "Homework",
                    Weighting = 25,
                    Category = AssessmentCategory.Assignment
                });
        }

        await context.SaveChangesAsync();

        return course;
    }

    private static async Task EnsureDashboardStudentCourseAsync(AppDbContext context, User student, Course course, Term semester2)
    {
        var studentCourse = await context.StudentCourses
            .FirstOrDefaultAsync(sc => sc.StudentId == student.Id && sc.CourseId == course.Id);

        if (studentCourse == null)
        {
            await context.StudentCourses.AddAsync(new StudentCourse
            {
                StudentId = student.Id,
                CourseId = course.Id,
                TermId = semester2.Id,
                AcademicYear = DashboardAcademicYear,
                Status = StudentCourseStatus.Completed,
                Grade = Grade.BPlus,
                Notes = "Dashboard demo course"
            });
        }
        else
        {
            studentCourse.TermId = semester2.Id;
            studentCourse.AcademicYear = DashboardAcademicYear;
            studentCourse.Status = StudentCourseStatus.Completed;
            studentCourse.Grade = Grade.BPlus;
            studentCourse.Notes = "Dashboard demo course";
        }

        await context.SaveChangesAsync();
    }
}
