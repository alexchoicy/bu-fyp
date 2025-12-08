using Backend.Data;
using Backend.Dtos.Facts;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Facts;

public interface IFactService
{
    Task<CurrentFactsResponseDto> GetCurrentFactsAsync();
    Task<List<CodeResponseDto>> GetCodesAsync();
    Task<List<CourseGroupResponseDto>> GetCourseGroupsAsync();
    Task<List<DepartmentResponseDto>> GetDepartmentsAsync();
    int GetCurrentSemester();
    int GetCurrentAcademicYear();
}

public class FactService : IFactService
{
    private readonly AppDbContext _context;

    public FactService(AppDbContext context)
    {
        _context = context;
    }

    public int GetCurrentSemester()
    {
        var month = DateTime.Now.Month;

        return month switch
        {
            >= 9 and <= 12 => 1,
            >= 1 and <= 6 => 2,
            _ => 3
        };
    }

    public int GetCurrentAcademicYear()
    {
        var now = DateTime.Now;
        var startYear = now.Month >= 9 ? now.Year : now.Year - 1;
        return startYear;
    }


    public async Task<CurrentFactsResponseDto> GetCurrentFactsAsync()
    {
        var currentSemester = GetCurrentSemester();
        var currentAcademicYear = GetCurrentAcademicYear();

        var term = await _context.Terms
            .FirstOrDefaultAsync(t => t.Name.Contains(currentSemester.ToString()));

        return new CurrentFactsResponseDto
        {
            CurrentSemester = currentSemester,
            CurrentAcademicYear = currentAcademicYear,
            CurrentTermName = term?.Name ?? $"Semester {currentSemester}"
        };
    }

    public async Task<List<CodeResponseDto>> GetCodesAsync()
    {
        return await _context.Codes
            .OrderBy(c => c.Tag)
            .Select(c => new CodeResponseDto
            {
                Id = c.Id,
                Tag = c.Tag,
                Name = c.Name
            })
            .ToListAsync();
    }

    public async Task<List<CourseGroupResponseDto>> GetCourseGroupsAsync()
    {
        return await _context.CourseGroups
            .OrderBy(g => g.Name)
            .Select(g => new CourseGroupResponseDto
            {
                Id = g.Id,
                Name = g.Name
            })
            .ToListAsync();
    }

    public async Task<List<DepartmentResponseDto>> GetDepartmentsAsync()
    {
        return await _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                Name = d.Name
            })
            .ToListAsync();
    }
}
