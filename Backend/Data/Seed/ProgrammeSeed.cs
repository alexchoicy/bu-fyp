using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class ProgrammeSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Programmes.AnyAsync())
            return;

        // Create Course Groups first
        var compSciCore001 = new CourseGroup { Name = "Core Courses - COMPSCI-CORE-001" };
        var freeElective001 = new CourseGroup { Name = "Free Elective Courses" };

        var compISAElective001 = new CourseGroup { Name = "ISA Elective Courses - COMP-ISA-ELEC-001" };
        var compISAElective002 = new CourseGroup { Name = "ISA Elective Courses - COMP-ISA-ELEC-002" };
        var compISAElective003 = new CourseGroup { Name = "ISA Elective Courses - COMP-ISA-ELEC-003" };

        var compCode = await context.Codes.FirstAsync(c => c.Tag == "COMP");
        var mathCode = await context.Codes.FirstAsync(c => c.Tag == "MATH");

        freeElective001.GroupCourses.Add(new GroupCourse
        {
            Code = compCode
        });
        freeElective001.GroupCourses.Add(new GroupCourse
        {
            Code = mathCode
        });


        await context.CourseGroups.AddRangeAsync(compSciCore001, freeElective001, compISAElective001, compISAElective002, compISAElective003);
        await context.SaveChangesAsync();

        var programme = new Programme
        {
            Name = "BSc Computer Science (ISA)",
            Description = @"Bachelor of Science in Computer Science with Information Systems and Applications concentration. 
                This programme prepares students for careers in software development, systems analysis, and IT management."
        };
        await context.Programmes.AddAsync(programme);

        var programmeVersion = new ProgrammeVersion
        {
            Programme = programme,
            VersionNumber = 1,
            StartYear = 2024,
            EndYear = 2025,
            IsActive = true,
            TotalCredits = 45
        };
        await context.ProgrammeVersions.AddAsync(programmeVersion);
        await context.SaveChangesAsync();

        var category0 = new Category
        {
            Name = "COMP SCI - ISA Core Courses",
            MinCredit = 15,
            Priority = 11,
            Rules = new RuleRuleNode
            {
                Operator = RuleOperator.And,
                Children = new List<RuleNode>
                {
                    new GroupRuleNode { GroupID = compSciCore001.Id, CourseSelectionMode = CourseSelectionMode.AllOf },
                }
            }
        };

        var category2 = new Category
        {
            Name = "Free Elective Courses",
            MinCredit = 24,
            Priority = -1,
            Rules = new FreeElectiveRuleNode
            {
                GroupID = freeElective001.Id,
                MinCredits = 24
            }
        };

        var categoryISAElective = new Category
        {
            Name = "ISA Elective Courses",
            MinCredit = 6,
            Priority = 5,
            Rules = new RuleRuleNode
            {
                Operator = RuleOperator.Any,
                Children = new List<RuleNode>
                {
                    new RuleRuleNode
                    {
                        Operator = RuleOperator.And,
                        Children = new List<RuleNode>
                        {
                            new GroupRuleNode { GroupID = compISAElective001.Id, CourseSelectionMode = CourseSelectionMode.OneOf},
                            new GroupRuleNode { GroupID = compISAElective002.Id, CourseSelectionMode = CourseSelectionMode.OneOf},
                        }
                    },
                    new RuleRuleNode
                    {
                        Operator = RuleOperator.And,
                        Children = new List<RuleNode>
                        {
                            new RuleRuleNode
                            {
                                Operator = RuleOperator.Any,
                                Children = new List<RuleNode>
                                {
                            new GroupRuleNode { GroupID = compISAElective001.Id, CourseSelectionMode = CourseSelectionMode.OneOf},
                            new GroupRuleNode { GroupID = compISAElective002.Id, CourseSelectionMode = CourseSelectionMode.OneOf},
                                }
                            },
                            new GroupRuleNode { GroupID = compISAElective003.Id, CourseSelectionMode = CourseSelectionMode.OneOf},
                        }
                    }
                }
            }
        };

        await context.Categories.AddRangeAsync(category0, category2, categoryISAElective);
        await context.SaveChangesAsync();

        var programmeVersionCategories = new List<ProgrammeCategory>
        {
            new ProgrammeCategory
            {
                ProgrammeVersion = programmeVersion,
                Category = category0
            },
            new ProgrammeCategory
            {
                ProgrammeVersion = programmeVersion,
                Category = category2
            },
            new ProgrammeCategory
            {
                ProgrammeVersion = programmeVersion,
                Category = categoryISAElective
            }
        };

        await context.ProgrammeCategories.AddRangeAsync(programmeVersionCategories);

        var categoryGroup = new List<CategoryGroup>
        {
            new() { Category = category0, Group = compSciCore001 },
            new() { Category = category2, Group = freeElective001 },
            new() { Category = categoryISAElective, Group = compISAElective001 },
            new() { Category = categoryISAElective, Group = compISAElective002 },
            new() { Category = categoryISAElective, Group = compISAElective003 }
        };
        await context.CategoryGroups.AddRangeAsync(categoryGroup);
        await context.SaveChangesAsync();

        await LinkStudentToProgrammeAsync(context, programmeVersion);

    }

    private static async Task LinkStudentToProgrammeAsync(AppDbContext context, ProgrammeVersion programmeVersion)
    {
        // Get the student user
        var studentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@bu.edu");
        if (studentUser == null)
        {
            Console.WriteLine("Student user not found. Skipping programme linking.");
            return;
        }

        var existingLink = await context.StudentProgrammes.FirstOrDefaultAsync(sp =>
            sp.StudentId == studentUser.Id && sp.ProgrammeVersionId == programmeVersion.Id);

        if (existingLink == null)
        {
            var studentProgramme = new StudentProgramme
            {
                StudentId = studentUser.Id,
                ProgrammeVersionId = programmeVersion.Id
            };
            context.StudentProgrammes.Add(studentProgramme);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }
}