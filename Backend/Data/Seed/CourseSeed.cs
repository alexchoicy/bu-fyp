using Backend.Models;
using Backend.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class CourseSeed
{
    public static async Task SeedAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        // if (await context.Courses.AnyAsync())
        //     return;

        // await seedCourseGroup(context);
        // await seedCourses(context, aiProviderFactory);
    }

    private static async Task seedCourseGroup(AppDbContext context)
    {
        var compSciCore001 = new CourseGroup { Name = "Core Courses - COMPSCI-CORE-001" };
        var freeElective001 = new CourseGroup { Name = "Free Elective Courses" };

        await context.CourseGroups.AddRangeAsync(compSciCore001, freeElective001);
        await context.SaveChangesAsync();
    }

    private static async Task GenerateCourseEmbeddings(CourseVersion courseVersion, List<CourseAssessment> assessments,
        IAIProvider aiProvider, Course course)
    {
        try
        {
            // Convert CourseAssessment to AssessmentMethod for embedding generation
            var assessmentMethods = assessments.Select(a => new AssessmentMethod
            {
                Name = a.Name,
                Weighting = a.Weighting,
                Category = a.Category,
                Description = ""
            }).ToList();

            // Generate domain tag embedding
            var domainEmbedding = await aiProvider.CreateCourseDomainTagEmbeddingAsync(
                courseTitle: course.Name,
                aimsAndObjectives: courseVersion.AimAndObjectives,
                courseContent: courseVersion.CourseContent
            );
            Console.WriteLine($"Generated domain embedding for course: {course.Name}");

            // Generate skills tag embedding
            var skillsEmbedding = await aiProvider.CreateCourseSkillsTagEmbeddingAsync(
                aimsAndObjectives: courseVersion.AimAndObjectives,
                cilos: courseVersion.CILOs,
                courseContent: courseVersion.CourseContent,
                tlas: courseVersion.TLAs,
                assessmentMethods: assessmentMethods
            );
            Console.WriteLine($"Generated skills embedding for course: {course.Name}");

            // Generate content types tag embedding
            var contentTypesEmbedding = await aiProvider.CreateCourseContentTypesTagEmbeddingAsync(
                courseContent: courseVersion.CourseContent,
                tlas: courseVersion.TLAs,
                assessmentMethods: assessmentMethods
            );
            Console.WriteLine($"Generated content types embedding for course: {course.Name}");

            // Store embeddings (for now, just logging - you can add storage logic later)
            Console.WriteLine($"Successfully generated all embeddings for: {course.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to generate embeddings for course {course.Name}: {ex.Message}");
        }
    }

    private static async Task seedCourses(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        var compCode = await context.Codes.FirstOrDefaultAsync(c => c.Tag == "COMP");
        var mathCode = await context.Codes.FirstOrDefaultAsync(c => c.Tag == "MATH");
        var semester1 = await context.Terms.FirstOrDefaultAsync(t => t.Name == "Semester 1");
        var englishMedium = await context.MediumOfInstructions.FirstOrDefaultAsync(m => m.Name == "English");
        var compSciCoreGroup =
            await context.CourseGroups.FirstOrDefaultAsync(g => g.Name == "Core Courses - COMPSCI-CORE-001");
        var freeElectiveGroup = await context.CourseGroups.FirstOrDefaultAsync(g => g.Name == "Free Elective Courses");


        var comp2045 = new Course
        {
            Name = "Programming and Problem Solving",
            CourseNumber = "2045",
            CodeId = compCode.Id,
            Credit = 2,
            Description = "Introduction to programming and problem solving using an object-oriented language"
        };

        await context.Courses.AddAsync(comp2045);

        var comp2045Version = new CourseVersion()
        {
            Course = comp2045,
            VersionNumber = 1,
            Description = "Programming fundamentals and problem solving with object-oriented approach",
            AimAndObjectives =
                "Study the programming basics of an object-oriented language and develop computer programs to solve practical problems",
            CourseContent =
                "Programming methodologies, lexical elements, data types, control structures, arrays, methods, classes and objects, problem formulation, code modularization, method reuse and overloading, testing and debugging, exception handling, and file I/O",
            CILOs = new List<CILOs>
            {
                new()
                {
                    code = "CILO1",
                    Description = "Describe the elements and basic syntax of an object-oriented language"
                },
                new()
                {
                    code = "CILO2",
                    Description =
                        "Describe the importance of programming styles, implementation and testing in programming"
                },
                new() { code = "CILO3", Description = "Design, develop and test computer programs" },
                new() { code = "CILO4", Description = "Formulate problems as systematic steps for problem solving" }
            },
            TLAs = new List<TLAs>
            {
                new()
                {
                    code = new[] { "Lecture" },
                    Description =
                        "Lectures introduce object-oriented programming concepts and problem-solving techniques"
                },
                new()
                {
                    code = new[] { "Laboratory", "Machine Problem" },
                    Description =
                        "Laboratories and machine problems allow students to apply programming knowledge to practical problems"
                }
            },
            FromYear = 2022,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp2045Version);

        var comp2045Aessments = new List<CourseAssessment>(new[]
        {
            new CourseAssessment
            {
                CourseVersion = comp2045Version,
                Name = "Laboratory Coding Exercises and Take-home Assignments",
                Weighting = 29,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = comp2045Version,
                Name = "Quizzes and Tests",
                Weighting = 31,
                Category = AssessmentCategory.Examination
            },
            new CourseAssessment
            {
                CourseVersion = comp2045Version,
                Name = "Final Examination",
                Weighting = 40,
                Category = AssessmentCategory.Examination
            }
        });
        await context.CourseAssessments.AddRangeAsync(comp2045Aessments);

        await context.SaveChangesAsync();

        // Generate embeddings for COMP2045
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            await GenerateCourseEmbeddings(comp2045Version, comp2045Aessments.ToList(), aiProvider, comp2045);
        }

        //

        var comp2046 = new Course
        {
            Name = "Problem Solving Using Object Oriented Approach",
            CourseNumber = "2046",
            CodeId = compCode.Id,
            Credit = 2,
            Description = "Object-oriented programming principles and techniques for problem solving"
        };

        await context.Courses.AddAsync(comp2046);

        var comp2046Version = new CourseVersion()
        {
            // Use navigation instead of CourseId to avoid FK issues before the course is saved
            Course = comp2046,
            VersionNumber = 1,
            Description = "Problem solving with object-oriented programming principles and techniques",
            AimAndObjectives =
                "Study object-oriented programming principles and techniques and apply them to solve practical problems using an object-oriented programming language",
            CourseContent =
                "Classes and objects, references and dynamic memory, static and final members, classification, generalization and specialization, object-oriented program construction, inheritance, polymorphism, interfaces, and abstract classes",
            CILOs = new List<CILOs>
            {
                new()
                {
                    code = "CILO1",
                    Description = "Describe the fundamentals and concepts of object-oriented programming"
                },
                new()
                {
                    code = "CILO2",
                    Description = "Apply object-oriented programming concepts to construct computer programs"
                },
                new()
                {
                    code = "CILO3", Description = "Formulate complex problems as modules for systematic problem solving"
                },
                new()
                {
                    code = "CILO4",
                    Description = "Integrate robustness, reusability, and portability into software development"
                }
            },
            TLAs = new List<TLAs>
            {
                new()
                {
                    code = new[] { "Lecture" },
                    Description =
                        "Lectures introduce object-oriented concepts, principles, and implementation techniques"
                },
                new()
                {
                    code = new[] { "Laboratory", "Machine Problem" },
                    Description =
                        "Laboratories and machine problems enable students to apply object-oriented techniques in program development"
                }
            },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(comp2046Version);

        var comp2046Aessments = new List<CourseAssessment>(new[]
        {
            new CourseAssessment
            {
                CourseVersion = comp2046Version,
                Name = "Laboratory Coding Exercises and Take-home Assignments",
                Weighting = 39,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = comp2046Version,
                Name = "Quizzes and Tests",
                Weighting = 21,
                Category = AssessmentCategory.Examination
            },
            new CourseAssessment
            {
                CourseVersion = comp2046Version,
                Name = "Final Examination",
                Weighting = 40,
                Category = AssessmentCategory.Examination
            }
        });

        await context.CourseAssessments.AddRangeAsync(comp2046Aessments);

        await context.SaveChangesAsync();

        // Generate embeddings for COMP2046
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            await GenerateCourseEmbeddings(comp2046Version, comp2046Aessments.ToList(), aiProvider, comp2046);
        }

        var math1025 = new Course()
        {
            Name = "Understanding Mathematics and Statistics",
            CourseNumber = "1025",
            CodeId = mathCode.Id,
            Credit = 3,
            Description =
                "Application of pre-university mathematics and statistics concepts to solve real-life problems with ICT tools"
        };

        await context.Courses.AddAsync(math1025);

        var math1025Version = new CourseVersion()
        {
            Course = math1025,
            VersionNumber = 1,
            Description = "Applied mathematics and statistics for real-life problem solving",
            AimAndObjectives = "Apply pre-university level mathematics and statistics concepts to solve real-life problems using problem-solving methodologies and hands-on ICT skills, and to develop statistical literacy for STEM, business, and everyday contexts",
            CourseContent = "Introduction to calculus as the study of change, data literacy and misleading statistics, data analysis and prediction, probability and randomness, numerical experiments, linear and nonlinear thinking, and applications using computer software",
            CILOs = new List<CILOs>
            {
                new() { code = "CILO1", Description = "Explain basic concepts and theories in mathematics and statistics" },
                new() { code = "CILO2", Description = "Illustrate basic applications of selected mathematics and statistics topics" },
                new() { code = "CILO3", Description = "Design spreadsheets to solve real-life mathematics and statistics problems" },
                new() { code = "CILO4", Description = "Construct perspectives for understanding mathematics and statistics" }
            },
            TLAs = new List<TLAs>
            {
                new()
                {
                    code = new[] { "Lecture" },
                    Description = "Lectures present motivating examples and introduce fundamental concepts and applications"
                },
                new()
                {
                    code = new[] { "Tutorial", "In-class Activity" },
                    Description = "In-class activities and tutorials involve discussions, exercises, and programming or software practices"
                },
                new()
                {
                    code = new[] { "After-class Activity" },
                    Description = "After-class tasks include online practice, research activities, and discussions with instructors and peers"
                }
            },
            FromYear = 2025,
            FromTermId = semester1.Id
        };

        await context.CourseVersions.AddAsync(math1025Version);

        var math1025Aessments = new List<CourseAssessment>(new[]
        {
            new CourseAssessment
            {
                CourseVersion = math1025Version,
                Name = "Online Homework",
                Weighting = 20,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = math1025Version,
                Name = "Written Assignments",
                Weighting = 20,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = math1025Version,
                Name = "Oral Assessment",
                Weighting = 20,
                Category = AssessmentCategory.Assignment
            },
            new CourseAssessment
            {
                CourseVersion = math1025Version,
                Name = "In-class Programming",
                Weighting = 20,
                Category = AssessmentCategory.Participation
            },
            new CourseAssessment
            {
                CourseVersion = math1025Version,
                Name = "Tests",
                Weighting = 20,
                Category = AssessmentCategory.Examination
            }
        });

        await context.CourseAssessments.AddRangeAsync(math1025Aessments);
        
        await context.SaveChangesAsync();

        // Generate embeddings for MATH1025
        if (aiProviderFactory != null)
        {
            var aiProvider = aiProviderFactory.GetProvider(AIProviderType.OpenAI);
            await GenerateCourseEmbeddings(math1025Version, math1025Aessments.ToList(), aiProvider, math1025);
        }

        var coreGroup = new List<GroupCourse>()
        {
            new GroupCourse()
            {
                Group = compSciCoreGroup,
                Course = comp2045
            },
            new GroupCourse()
            {
                Group = compSciCoreGroup,
                Course = comp2046
            },
            new GroupCourse()
            {
                Group = freeElectiveGroup,
                Course = math1025
            }
        };
        
        await context.GroupCourses.AddRangeAsync(coreGroup);
        await context.SaveChangesAsync();
    }
}