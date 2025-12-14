using Backend.Data;
using Backend.Dtos.Courses;
using Backend.Dtos.User;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.User;

public interface IUserService
{
    Task<List<UserCourseDto>> GetStudentStudyCoursesAsync(string userId);
    Task<bool> AddStudentStudyCoursesAsync(string userId, List<CreateStudentCourseDto> courses);
    Task<AcademicProgressDto> GetAcademicProgressAsync(string userId);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserCourseDto>> GetStudentStudyCoursesAsync(string userId)
    {
        var studentCourses = await _context.Set<StudentCourse>()
            .Where(sc => sc.StudentId == userId)
            .Include(sc => sc.Course)
            .Include(sc => sc.Term)
            .Include(sc => sc.Course.Code)
            .OrderByDescending(sc => sc.AcademicYear)
            .ThenByDescending(sc => sc.Term.Id)
            .ToListAsync();

        var userCourses = studentCourses.Select(sc => new UserCourseDto
        {
            CourseId = sc.CourseId,
            CourseName = sc.Course.Name,
            CourseNumber = sc.Course.CourseNumber,
            CodeId = sc.Course.CodeId,
            CodeTag = sc.Course.Code?.Tag ?? string.Empty,
            Credit = sc.Course.Credit,
            Status = sc.Status,
            Grade = sc.Grade,
            Term = sc.Term.Name,
            TermNumber = sc.Term.Id,
            AcademicYear = sc.AcademicYear,
            Notes = sc.Notes,
        }).ToList();

        return userCourses;
    }

    public async Task<bool> AddStudentStudyCoursesAsync(string userId, List<CreateStudentCourseDto> courses)
    {
        try
        {         
            var studentCoursesToAdd = new List<StudentCourse>();

            foreach (var courseDto in courses)
            {
                Models.Course? course = await _context.Courses.FindAsync(courseDto.CourseId);
                
                if (course == null)
                {
                    continue;
                }
                Console.WriteLine(courseDto.ToString());
                Console.WriteLine(course);
                
                var studentCourse = new StudentCourse
                {
                    StudentId = userId,
                    Course =  course,
                    TermId = courseDto.TermId,
                    AcademicYear = courseDto.AcademicYear,
                    Grade = courseDto.Grade,
                    Status = courseDto.Status,
                    Notes = courseDto.Notes
                };

                studentCoursesToAdd.Add(studentCourse);
            }

            _context.Set<StudentCourse>().AddRange(studentCoursesToAdd);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AcademicProgressDto> GetAcademicProgressAsync(string userId)
    {
        var progress = new AcademicProgressDto();

        var studentCourses = await _context.Set<StudentCourse>()
            .Where(sc => sc.StudentId == userId)
            .Include(sc => sc.Course)
            .Include(sc => sc.Term)
            .ToListAsync();
        
        var completedCourses = studentCourses.Where(sc => sc.Status == StudentCourseStatus.Completed).ToList();

        var studentProgrammes = await _context.Set<StudentProgramme>()
            .Where(sp => sp.StudentId == userId)
            .Include(sp => sp.ProgrammeVersion.Programme)
            .FirstOrDefaultAsync();

        var semesterGroups = completedCourses
            .GroupBy(sc => new { sc.AcademicYear, sc.TermId, TermName = sc.Term.Name })
            .OrderBy(g => g.Key.AcademicYear)
            .ThenBy(g => g.Key.TermId);

        decimal totalGradePoints = 0;
        int totalCreditsForGpa = 0;

        foreach (var semesterGroup in semesterGroups)
        {
            decimal semesterGradePoints = 0;
            int semesterCredits = 0;

            foreach (var course in semesterGroup)
            {
                if (course.Grade.HasValue && GradeUtility.GradePoints.TryGetValue(course.Grade.Value, out var gradePoint) && gradePoint.HasValue)
                {
                    semesterGradePoints += gradePoint.Value * course.Course.Credit;
                    semesterCredits += course.Course.Credit;
                    totalGradePoints += gradePoint.Value * course.Course.Credit;
                    totalCreditsForGpa += course.Course.Credit;
                }
            }

            decimal semesterGpa = semesterCredits > 0 ? semesterGradePoints / semesterCredits : 0;

            progress.SemesterGpas.Add(new SemesterGpaDto
            {
                Year = semesterGroup.Key.AcademicYear,
                Semester = semesterGroup.Key.TermId,
                Gpa = Math.Round(semesterGpa, 2),
                CreditsCompleted = semesterCredits,
                TermName = semesterGroup.Key.TermName
            });
        }

        progress.OverallGpa = totalCreditsForGpa > 0 ? Math.Round(totalGradePoints / totalCreditsForGpa, 2) : 0;

        progress.TotalCreditsCompleted = completedCourses.Sum(sc => sc.Course.Credit);
        progress.EnrolledCoursesCount = studentCourses.Count(sc => sc.Status == StudentCourseStatus.Enrolled);

        if (studentProgrammes != null)
        {
            progress.ProgrammeName = studentProgrammes.ProgrammeVersion.Programme.Name;
            progress.TotalCreditsRequired = studentProgrammes.ProgrammeVersion.TotalCredits;
        }

        return progress;
    }
}

