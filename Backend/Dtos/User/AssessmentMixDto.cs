namespace Backend.Dtos.User;

public class AssessmentMixCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal ExposureCredits { get; set; }
    public decimal ExposurePercentage { get; set; }
    public decimal PerformanceGpa { get; set; }
    public int AssessmentCount { get; set; }
    public int CourseCount { get; set; }
}

public class AssessmentMixInsightDto
{
    public string BestCategory { get; set; } = string.Empty;
    public decimal BestCategoryGpa { get; set; }
    public string WeakestCategory { get; set; } = string.Empty;
    public decimal WeakestCategoryGpa { get; set; }
}

public class AssessmentMixResponseDto
{
    public int CoverageStartYear { get; set; }
    public int CoverageStartTermId { get; set; }
    public string CoverageStartTermName { get; set; } = string.Empty;
    public int CoverageEndYear { get; set; }
    public int CoverageEndTermId { get; set; }
    public string CoverageEndTermName { get; set; } = string.Empty;
    public int CourseCount { get; set; }
    public int TermCount { get; set; }
    public decimal TotalExposureCredits { get; set; }
    public AssessmentMixInsightDto? Insight { get; set; }
    public List<AssessmentMixCategoryDto> Categories { get; set; } = new();
}
