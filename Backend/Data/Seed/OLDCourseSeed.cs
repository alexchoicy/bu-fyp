using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class OLDCourseSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedCoursesAsync(context);
    }

    private static async Task SeedCoursesAsync(AppDbContext context)
    {
        if (await context.Courses.AnyAsync())
            return;

        // Get required dependencies
        var compCode = await context.Codes.FirstOrDefaultAsync(c => c.Tag == "COMP");
        var mathCode = await context.Codes.FirstOrDefaultAsync(c => c.Tag == "MATH");
        var semester1 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 1");
        var semester2 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 2");
        var englishMedium = await context.MediumOfInstructions.FirstOrDefaultAsync(m => m.Name == "English");

        if (compCode == null || mathCode == null || semester1 == null || semester2 == null || englishMedium == null)
        {
            Console.WriteLine("Error: Required dependencies not found. Please seed Terms, Codes, and MediumOfInstructions first.");
            return;
        }

        // Create the 3 courses
        var courses = new List<Course>
        {
            new()
            {
                Name = "Information Systems: Design and Integration",
                CourseNumber = "4117",
                CodeId = compCode.Id,
                Credit = 3,
                Description = "Design and integration of information systems for enterprise environments"
            },
            new()
            {
                Name = "Database Management",
                CourseNumber = "2016",
                CodeId = compCode.Id,
                Credit = 3,
                Description = "Fundamentals of database design, management, and SQL"
            },
            new()
            {
                Name = "Calculus, Probability, and Statistics for Computer Science",
                CourseNumber = "2005",
                CodeId = mathCode.Id,
                Credit = 3,
                Description = "Calculus, probability theory, and statistical methods for computer science applications"
            }
        };

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();

        // Refresh to get IDs
        var allCourses = await context.Courses.ToListAsync();

        // Get the 3 specific courses we just created
        var comp4117 = allCourses.FirstOrDefault(c => c.CourseNumber == "4117");
        var comp2016 = allCourses.FirstOrDefault(c => c.CourseNumber == "2016");
        var math2005 = allCourses.FirstOrDefault(c => c.CourseNumber == "2005");

        if (comp4117 == null || comp2016 == null || math2005 == null)
        {
            Console.WriteLine("Error: Failed to retrieve created courses.");
            return;
        }

        // Create course versions for both semesters
        var courseVersions = new List<CourseVersion>
        {
            // COMP4117 - Semester 2 2024-2025
            new()
            {
                CourseId = comp4117.Id,
                VersionNumber = 1,
                Description = "Design and integration of enterprise information systems",
                AimAndObjectives = "Students will learn to design, integrate, and manage enterprise information systems",
                CourseContent = "Systems design methodologies, enterprise architecture, integration patterns, implementation strategies",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Understand enterprise information system design principles" },
                    new() { code = "CILO2", Description = "Design integrated information systems" },
                    new() { code = "CILO3", Description = "Evaluate system integration approaches" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Case Study" }, Description = "Lectures with real-world case studies" },
                    new() { code = new[] { "Project" }, Description = "Capstone system design project" }
                },
                FromYear = 2024,
                FromTermId = semester2.Id
            },
            // COMP2016 - Semester 2 2024-2025
            new()
            {
                CourseId = comp2016.Id,
                VersionNumber = 1,
                Description = "Fundamentals of database design and management",
                AimAndObjectives = "Master database design, implementation, and management concepts",
                CourseContent = "Database models, relational algebra, SQL, normalization, query optimization, transaction management",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Design efficient database schemas" },
                    new() { code = "CILO2", Description = "Write and optimize SQL queries" },
                    new() { code = "CILO3", Description = "Manage database systems" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Lab" }, Description = "Lectures with hands-on SQL practice" },
                    new() { code = new[] { "Assignment" }, Description = "Database design and implementation assignments" }
                },
                FromYear = 2024,
                FromTermId = semester2.Id
            },
            // MATH2005 - Semester 2 2024-2025
            new()
            {
                CourseId = math2005.Id,
                VersionNumber = 1,
                Description = "Calculus, probability, and statistics for computer science",
                AimAndObjectives = "Develop mathematical foundations for computer science applications",
                CourseContent = "Calculus fundamentals, probability theory, statistical inference, distributions, hypothesis testing",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Apply calculus concepts in CS contexts" },
                    new() { code = "CILO2", Description = "Understand probability and statistical methods" },
                    new() { code = "CILO3", Description = "Analyze data using statistical techniques" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Tutorial" }, Description = "Lectures and problem-solving tutorials" },
                    new() { code = new[] { "Assignment", "Exam" }, Description = "Regular assignments and examinations" }
                },
                FromYear = 2024,
                FromTermId = semester2.Id
            },
            // COMP4117 - Semester 1 2025-2026
            new()
            {
                CourseId = comp4117.Id,
                VersionNumber = 2,
                Description = "Design and integration of enterprise information systems",
                AimAndObjectives = "Students will learn to design, integrate, and manage enterprise information systems",
                CourseContent = "Systems design methodologies, enterprise architecture, integration patterns, implementation strategies",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Understand enterprise information system design principles" },
                    new() { code = "CILO2", Description = "Design integrated information systems" },
                    new() { code = "CILO3", Description = "Evaluate system integration approaches" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Case Study" }, Description = "Lectures with real-world case studies" },
                    new() { code = new[] { "Project" }, Description = "Capstone system design project" }
                },
                FromYear = 2025,
                FromTermId = semester1.Id
            },
            // COMP2016 - Semester 1 2025-2026
            new()
            {
                CourseId = comp2016.Id,
                VersionNumber = 2,
                Description = "Fundamentals of database design and management",
                AimAndObjectives = "Master database design, implementation, and management concepts",
                CourseContent = "Database models, relational algebra, SQL, normalization, query optimization, transaction management",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Design efficient database schemas" },
                    new() { code = "CILO2", Description = "Write and optimize SQL queries" },
                    new() { code = "CILO3", Description = "Manage database systems" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Lab" }, Description = "Lectures with hands-on SQL practice" },
                    new() { code = new[] { "Assignment" }, Description = "Database design and implementation assignments" }
                },
                FromYear = 2025,
                FromTermId = semester1.Id
            },
            // MATH2005 - Semester 1 2025-2026
            new()
            {
                CourseId = math2005.Id,
                VersionNumber = 2,
                Description = "Calculus, probability, and statistics for computer science",
                AimAndObjectives = "Develop mathematical foundations for computer science applications",
                CourseContent = "Calculus fundamentals, probability theory, statistical inference, distributions, hypothesis testing",
                CILOs = new List<CILOs>
                {
                    new() { code = "CILO1", Description = "Apply calculus concepts in CS contexts" },
                    new() { code = "CILO2", Description = "Understand probability and statistical methods" },
                    new() { code = "CILO3", Description = "Analyze data using statistical techniques" }
                },
                TLAs = new List<TLAs>
                {
                    new() { code = new[] { "Lecture", "Tutorial" }, Description = "Lectures and problem-solving tutorials" },
                    new() { code = new[] { "Assignment", "Exam" }, Description = "Regular assignments and examinations" }
                },
                FromYear = 2025,
                FromTermId = semester1.Id
            }
        };

        await context.CourseVersions.AddRangeAsync(courseVersions);
        await context.SaveChangesAsync();

        // Add course version mediums for all versions
        var versions = await context.CourseVersions.ToListAsync();
        var courseVersionMediums = new List<CourseVersionMedium>();

        foreach (var version in versions)
        {
            courseVersionMediums.Add(new CourseVersionMedium
            {
                CourseVersionId = version.Id,
                MediumOfInstructionId = englishMedium.Id
            });
        }

        await context.CourseVersionMediums.AddRangeAsync(courseVersionMediums);
        await context.SaveChangesAsync();

        // Add assessments for each course version
        var assessments = new List<CourseAssessment>();

        foreach (var version in versions)
        {
            // Determine which course this is based on CourseId
            var courseId = version.CourseId;
            var course = allCourses.FirstOrDefault(c => c.Id == courseId);

            if (course?.CourseNumber == "4117")
            {
                // COMP4117 assessments
                assessments.AddRange(new[]
                {
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "System Design Project",
                        Weighting = 50,
                        Category = AssessmentCategory.Project
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Case Study Analysis",
                        Weighting = 30,
                        Category = AssessmentCategory.Assignment
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Final Presentation",
                        Weighting = 20,
                        Category = AssessmentCategory.Presentation
                    }
                });
            }
            else if (course?.CourseNumber == "2016")
            {
                // COMP2016 assessments
                assessments.AddRange(new[]
                {
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Database Design Assignment",
                        Weighting = 35,
                        Category = AssessmentCategory.Assignment
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "SQL Query Assignments",
                        Weighting = 25,
                        Category = AssessmentCategory.Assignment
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Midterm Examination",
                        Weighting = 20,
                        Category = AssessmentCategory.Examination
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Final Examination",
                        Weighting = 20,
                        Category = AssessmentCategory.Examination
                    }
                });
            }
            else if (course?.CourseNumber == "2005")
            {
                // MATH2005 assessments
                assessments.AddRange(new[]
                {
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Problem Set Assignments",
                        Weighting = 30,
                        Category = AssessmentCategory.Assignment
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Midterm Examination",
                        Weighting = 35,
                        Category = AssessmentCategory.Examination
                    },
                    new CourseAssessment
                    {
                        CourseVersionId = version.Id,
                        Name = "Final Examination",
                        Weighting = 35,
                        Category = AssessmentCategory.Examination
                    }
                });
            }
        }

        await context.CourseAssessments.AddRangeAsync(assessments);
        await context.SaveChangesAsync();
    }
}

