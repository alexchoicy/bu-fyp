namespace Backend.Models;

/// <summary>
/// Letter grades for academic performance
/// </summary>
public enum Grade
{
    /// <summary>Excellent - 4.00 GPA</summary>
    A,
    
    /// <summary>Excellent - 3.67 GPA</summary>
    AMinus,
    
    /// <summary>Good - 3.33 GPA</summary>
    BPlus,
    
    /// <summary>Good - 3.00 GPA</summary>
    B,
    
    /// <summary>Good - 2.67 GPA</summary>
    BMinus,
    
    /// <summary>Satisfactory - 2.33 GPA</summary>
    CPlus,
    
    /// <summary>Satisfactory - 2.00 GPA</summary>
    C,
    
    /// <summary>Satisfactory - 1.67 GPA</summary>
    CMinus,
    
    /// <summary>Marginal Pass - 1.00 GPA</summary>
    D,
    
    /// <summary>Conditional Pass - 0.00 GPA</summary>
    E,
    
    /// <summary>Failure - 0.00 GPA</summary>
    F,
    
    /// <summary>Distinction - Not Included in GPA</summary>
    DT,
    
    /// <summary>Incomplete - Not Included in GPA</summary>
    I,
    
    /// <summary>Satisfactory - Not Included in GPA</summary>
    S,
    
    /// <summary>Unsatisfactory - Not Included in GPA</summary>
    U,
    
    /// <summary>Withdrawn - Not Included in GPA</summary>
    W,
    
    /// <summary>In Progress / Ongoing - Not Included in GPA</summary>
    IP,
    
    /// <summary>Not Assigned / Planned - Not Included in GPA</summary>
    NA
}

/// <summary>
/// Utility class for grade-related operations
/// </summary>
public static class GradeUtility
{

    /// <summary>
    /// Maps string representation to grade enum values
    /// </summary>
    public static readonly Dictionary<string, Grade> StringToGrade = new()
    {
        { "A", Grade.A },
        { "A-", Grade.AMinus },
        { "B+", Grade.BPlus },
        { "B", Grade.B },
        { "B-", Grade.BMinus },
        { "C+", Grade.CPlus },
        { "C", Grade.C },
        { "C-", Grade.CMinus },
        { "D", Grade.D },
        { "E", Grade.E },
        { "F", Grade.F },
        { "DT", Grade.DT },
        { "I", Grade.I },
        { "S", Grade.S },
        { "U", Grade.U },
        { "W", Grade.W },
        { "IP", Grade.IP },
        { "NA", Grade.NA }
    };

    /// <summary>
    /// Maps grade to their grade point value
    /// </summary>
    public static readonly Dictionary<Grade, decimal?> GradePoints = new()
    {
        { Grade.A, 4.00m },
        { Grade.AMinus, 3.67m },
        { Grade.BPlus, 3.33m },
        { Grade.B, 3.00m },
        { Grade.BMinus, 2.67m },
        { Grade.CPlus, 2.33m },
        { Grade.C, 2.00m },
        { Grade.CMinus, 1.67m },
        { Grade.D, 1.00m },
        { Grade.E, 0.00m },
        { Grade.F, 0.00m },
        { Grade.DT, null }, // Not included in GPA
        { Grade.I, null },  // Not included in GPA
        { Grade.S, null },  // Not included in GPA
        { Grade.U, null },  // Not included in GPA
        { Grade.W, null },  // Not included in GPA
        { Grade.IP, null }, // Not included in GPA
        { Grade.NA, null }  // Not included in GPA
    };
    
    // public static readonly Dictionary<Grade, string> PerformanceDescriptions = new()
    // {
    //     { Grade.A, "A" },
    //     { Grade.AMinus, "A-" },
    //     { Grade.BPlus, "B+" },
    //     { Grade.B, "B" },
    //     { Grade.BMinus, "B-" },
    //     { Grade.CPlus, "C+" },
    //     { Grade.C, "C" },
    //     { Grade.CMinus, "C-" },
    //     { Grade.D, "D" },
    //     { Grade.E, "Conditional Pass" },
    //     { Grade.F, "F" },
    //     { Grade.DT, "Distinction" },
    //     { Grade.I, "Incomplete" },
    //     { Grade.S, "Satisfactory" },
    //     { Grade.U, "Unsatisfactory" },
    //     { Grade.W, "Withdrawn" },
    //     { Grade.NA, "Planned / Not Assigned" }
    // };

    public static bool IsPassing(Grade grade)
    {
        return grade switch
        {
            Grade.A or Grade.AMinus or Grade.BPlus or Grade.B or Grade.BMinus 
                or Grade.CPlus or Grade.C or Grade.CMinus or Grade.D or Grade.E 
                or Grade.DT or Grade.S => true,
            _ => false
        };
    }

    public static decimal? GetGradePoint(Grade grade)
    {
        return GradePoints.TryGetValue(grade, out var point) ? point : null;
    }
}

