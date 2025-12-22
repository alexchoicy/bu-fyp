using System.Collections.Generic;
using Backend.Models;
using Pgvector;

namespace Backend.Services.AI;

public class GeminiProvider : IAIProvider
{
    public Task<List<AssessmentMethod>> ExtractAssessmentMethodsAsync(string text)
    {
        throw new NotImplementedException();
    }

    public Task<Vector> CreateEmbeddingAsync(string text)
    {
        throw new NotImplementedException();
    }

    public async Task<Vector> CreateCourseDomainTagEmbeddingAsync(string courseTitle, string aimsAndObjectives, string courseContent)
    {
        try
        {
            // Format course data using the shared helper
            string formattedText = EmbeddingHelper.FormatCourseDataForDomainTagEmbedding(courseTitle, aimsAndObjectives, courseContent);
            
            // Create and return the embedding
            return await CreateEmbeddingAsync(formattedText);
        }
        catch (NotImplementedException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error occurred while creating course embedding with Gemini provider", e);
        }
    }

    public async Task<Vector> CreateCourseSkillsTagEmbeddingAsync(string aimsAndObjectives, List<CILOs> cilos, string courseContent, List<TLAs> tlas, List<AssessmentMethod> assessmentMethods)
    {
        try
        {
            // Format course data using the shared helper
            string formattedText = EmbeddingHelper.FormatCourseDataForSkillsTagEmbedding(aimsAndObjectives, cilos, courseContent, tlas, assessmentMethods);
            
            // Create and return the embedding
            return await CreateEmbeddingAsync(formattedText);
        }
        catch (NotImplementedException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error occurred while creating course skills tag embedding with Gemini provider", e);
        }
    }

    public async Task<Vector> CreateCourseContentTypesTagEmbeddingAsync(string courseContent, List<TLAs> tlas, List<AssessmentMethod> assessmentMethods)
    {
        try
        {
            // Format course data using the shared helper
            string formattedText = EmbeddingHelper.FormatCourseDataForContentTypesTagEmbedding(courseContent, tlas, assessmentMethods);
            
            // Create and return the embedding
            return await CreateEmbeddingAsync(formattedText);
        }
        catch (NotImplementedException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error occurred while creating course content types tag embedding with Gemini provider", e);
        }
    }

    public Task<List<CILOs>> ExtractCILOsAsync(string text)
    {
        throw new NotImplementedException();
    }

    public Task<List<TLAs>> ExtractTLAsAsync(string text)
    {
        throw new NotImplementedException();
    }
}
