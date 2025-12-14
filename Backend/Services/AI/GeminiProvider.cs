using Backend.Models;

namespace Backend.Services.AI;

public class GeminiProvider : IAIProvider
{
    public Task<List<AssessmentMethod>> ExtractAssessmentMethodsAsync(string text)
    {
        throw new NotImplementedException();
    }

    public Task<float[]> CreateEmbeddingAsync(string text)
    {
        throw new NotImplementedException();
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
