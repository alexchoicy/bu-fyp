namespace Backend.Dtos.User;


public class SemesterGpaDto
{

    public int Year { get; set; }


    public int Semester { get; set; }


    public decimal Gpa { get; set; }
    
    public int CreditsCompleted { get; set; }
    
    public string TermName { get; set; } = string.Empty;
}


public class AcademicProgressDto
{

    public List<SemesterGpaDto> SemesterGpas { get; set; } = new();


    public decimal OverallGpa { get; set; }


    public int TotalCreditsCompleted { get; set; }


    public int TotalCreditsRequired { get; set; }


    public string? ProgrammeName { get; set; }

    public int EnrolledCoursesCount { get; set; }

    public int RemainingCredits => TotalCreditsRequired - TotalCreditsCompleted;
    
    public decimal CompletionPercentage => TotalCreditsRequired > 0 
        ? Math.Round((decimal)TotalCreditsCompleted / TotalCreditsRequired * 100, 2)
        : 0;
}

