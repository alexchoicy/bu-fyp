using Backend.Models;
using Backend.Services.AI;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seed;

public class DataSeed
{
    public static async Task SeedAsync(AppDbContext context, IAIProviderFactory? aiProviderFactory = null)
    {
        await SeedTermsAsync(context);
        await SeedCodesAsync(context);
        await SeedDepartmentsAsync(context);
        await SeedMediumOfInstructionsAsync(context);

        // This seed will use SQL bulk input after first embedding generation
        // await TagSeed.SeedAsync(context, aiProviderFactory);
        // await PolicySeed.SeedAsync(context, aiProviderFactory);

        // await CourseSeed.SeedAsync(context, aiProviderFactory);
        // await SectionSeed.SeedAsync(context);
        await ProgrammeSeed.SeedAsync(context);
    }

    private static async Task SeedTermsAsync(AppDbContext context)
    {
        if (await context.Terms.AnyAsync())
            return;

        var terms = new List<Term>
        {
            new() { Name = "Semester 1" },
            new() { Name = "Semester 2" },
            new() { Name = "Summer Term" }
        };

        await context.Terms.AddRangeAsync(terms);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCodesAsync(AppDbContext context)
    {
        if (await context.Codes.AnyAsync())
            return;

        var codes = new List<Code>
        {
            new() { Tag = "COMP", Name = "Computing" },
            new() { Tag = "MATH", Name = "Mathematics" },
            new() { Tag = "GCAP", Name = "General Education Capstone" }
        };

        await context.Codes.AddRangeAsync(codes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(AppDbContext context)
    {
        if (await context.Departments.AnyAsync())
            return;

        var departments = new List<Department>
        {
            new() { Name = "Department of Computer Science" },
            new() { Name = "Department of Mathematics" },
            new() { Name = "Department of English" },
            new() { Name = "Department of History" }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMediumOfInstructionsAsync(AppDbContext context)
    {
        if (await context.MediumOfInstructions.AnyAsync())
            return;

        var mediums = new List<MediumOfInstruction>
        {
            new() { Name = "English" },
            new() { Name = "Cantonese" },
            new() { Name = "Putonghua" },
            new() { Name = "Japanese" }

        };

        await context.MediumOfInstructions.AddRangeAsync(mediums);
        await context.SaveChangesAsync();
    }
}
