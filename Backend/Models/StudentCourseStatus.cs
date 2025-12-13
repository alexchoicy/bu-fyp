using System.Text.Json.Serialization;

namespace Backend.Models;

public enum StudentCourseStatus
{
    Enrolled,
    Completed,
    Dropped,
    Planned,
    Withdrawn,
    Failed,
    Exemption
}
