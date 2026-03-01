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
    Task<SuggestedScheduleResponseDto> GetSuggestedScheduleAsync(string userId);
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
        
        var completedCourses = studentCourses.Where(sc => 
            sc.Status == StudentCourseStatus.Completed || 
            sc.Status == StudentCourseStatus.Exemption || 
            sc.Status == StudentCourseStatus.Withdrawn).ToList();
        
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

    public async Task<SuggestedScheduleResponseDto> GetSuggestedScheduleAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var currentStudyYear = user?.GetCurrentStudyYear() ?? 0;

        var month = DateTime.Now.Month;
        var currentTermId = month switch
        {
            >= 9 and <= 12 => 1,
            >= 1 and <= 6 => 2,
            _ => 3,
        };

        var currentTermName = await _context.Terms
            .Where(term => term.Id == currentTermId)
            .Select(term => term.Name)
            .FirstOrDefaultAsync()
            ?? $"Semester {currentTermId}";

        var programmeVersionId = await _context.Set<StudentProgramme>()
            .Where(sp => sp.StudentId == userId)
            .Select(sp => sp.ProgrammeVersionId)
            .FirstOrDefaultAsync();

        if (programmeVersionId == 0)
        {
            return new SuggestedScheduleResponseDto
            {
                CurrentStudyYear = currentStudyYear,
                CurrentTermId = currentTermId,
                CurrentTermName = currentTermName,
            };
        }

        var scheduleRows = await _context.Set<ProgrammeSuggestedCourseSchedule>()
            .Where(ps => ps.ProgrammeVersionId == programmeVersionId)
            .Include(ps => ps.Term)
            .Include(ps => ps.Course)
                .ThenInclude(c => c!.Code)
            .OrderBy(ps => ps.StudyYear)
            .ThenBy(ps => ps.TermId)
            .ThenBy(ps => ps.Id)
            .ToListAsync();

        var years = scheduleRows
            .GroupBy(ps => ps.StudyYear)
            .OrderBy(group => group.Key)
            .Select(yearGroup => new SuggestedScheduleYearDto
            {
                StudyYear = yearGroup.Key,
                Terms = yearGroup
                    .GroupBy(ps => new { ps.TermId, ps.Term.Name })
                    .OrderBy(termGroup => termGroup.Key.TermId)
                    .Select(termGroup => new SuggestedScheduleTermDto
                    {
                        TermId = termGroup.Key.TermId,
                        TermName = termGroup.Key.Name,
                        Items = termGroup
                            .Select(ps => new SuggestedScheduleItemDto
                            {
                                Id = ps.Id,
                                StudyYear = ps.StudyYear,
                                TermId = ps.TermId,
                                TermName = ps.Term.Name,
                                CourseId = ps.CourseId,
                                CourseCode = ps.Course?.Code?.Tag,
                                CourseNumber = ps.Course?.CourseNumber,
                                CourseName = ps.Course?.Name,
                                CourseCredit = ps.Course?.Credit,
                                IsCoreElective = ps.IsCoreElective,
                                IsFreeElective = ps.IsFreeElective,
                                Credits = ps.Credits,
                                ItemType = ps.IsFreeElective
                                    ? "FreeElective"
                                    : ps.IsCoreElective
                                        ? "CoreElective"
                                        : "Course",
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .ToList();

        return new SuggestedScheduleResponseDto
        {
            CurrentStudyYear = currentStudyYear,
            CurrentTermId = currentTermId,
            CurrentTermName = currentTermName,
            Years = years,
        };
    }
}
