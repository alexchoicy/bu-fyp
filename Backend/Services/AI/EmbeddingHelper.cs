namespace Backend.Services.AI;

using Backend.Models;

public static class EmbeddingHelper
{
    public static string FormatCourseDataForDomainTagEmbedding(
        string? courseTitle,
        string? aimsAndObjectives,
        string? courseContent)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(courseTitle))
        {
            parts.Add($"COURSE TITLE: {courseTitle.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(aimsAndObjectives))
        {
            parts.Add($"AIMS & OBJECTIVES: {aimsAndObjectives.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(courseContent))
        {
            parts.Add($"COURSE CONTENT: {courseContent.Trim()}");
        }

        return string.Join("\n\n", parts);
    }

    public static string FormatCourseDataForSkillsTagEmbedding(
        string? aimsAndObjectives,
        List<CILOs>? cilos,
        string? courseContent,
        List<TLAs>? tlas,
        List<AssessmentMethod>? assessmentMethods)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(aimsAndObjectives))
        {
            parts.Add($"AIMS & OBJECTIVES: {aimsAndObjectives.Trim()}");
        }

        if (cilos != null && cilos.Count > 0)
        {
            var cilosText = string.Join("; ", cilos.Select(c =>
                $"{c.code}: {c.Description}".Trim()));
            if (!string.IsNullOrWhiteSpace(cilosText))
            {
                parts.Add($"COURSE INTENDED LEARNING OUTCOMES (CILOs): {cilosText}");
            }
        }

        if (!string.IsNullOrWhiteSpace(courseContent))
        {
            parts.Add($"COURSE CONTENT: {courseContent.Trim()}");
        }

        if (tlas != null && tlas.Count > 0)
        {
            var tlasText = string.Join("; ", tlas.Select(t =>
                $"{string.Join(",", t.code)}: {t.Description}".Trim()));
            if (!string.IsNullOrWhiteSpace(tlasText))
            {
                parts.Add($"TEACHING & LEARNING ACTIVITIES (TLAs): {tlasText}");
            }
        }

        if (assessmentMethods != null && assessmentMethods.Count > 0)
        {
            var assessmentText = string.Join("; ", assessmentMethods.Select(a =>
                $"{a.Name} ({a.Category}, {a.Weighting}%): {a.Description}".Trim()));
            if (!string.IsNullOrWhiteSpace(assessmentText))
            {
                parts.Add($"ASSESSMENT METHODS (AMs): {assessmentText}");
            }
        }

        return string.Join("\n\n", parts);
    }

    public static string FormatCourseDataForContentTypesTagEmbedding(
        string? courseContent,
        List<TLAs>? tlas,
        List<AssessmentMethod>? assessmentMethods)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(courseContent))
        {
            parts.Add($"COURSE CONTENT: {courseContent.Trim()}");
        }

        if (tlas != null && tlas.Count > 0)
        {
            var tlasText = string.Join("; ", tlas.Select(t =>
                $"{string.Join(",", t.code)}: {t.Description}".Trim()));
            if (!string.IsNullOrWhiteSpace(tlasText))
            {
                parts.Add($"TEACHING & LEARNING ACTIVITIES (TLAs): {tlasText}");
            }
        }

        if (assessmentMethods != null && assessmentMethods.Count > 0)
        {
            var assessmentText = string.Join("; ", assessmentMethods.Select(a =>
                $"{a.Name} ({a.Category}, {a.Weighting}%): {a.Description}".Trim()));
            if (!string.IsNullOrWhiteSpace(assessmentText))
            {
                parts.Add($"ASSESSMENT METHODS (AMs): {assessmentText}");
            }
        }

        return string.Join("\n\n", parts);
    }
}

