using Backend.Models;
using OpenAI.Chat;
using Pgvector;

namespace Backend.Services.AI;

public enum AIProviderType
{
    OpenAI,
    Gemini
}

public interface IAIProvider
{
    Task<List<CILOs>> ExtractCILOsAsync(string text);

    Task<List<TLAs>> ExtractTLAsAsync(string text);

    Task<List<AssessmentMethod>> ExtractAssessmentMethodsAsync(string text);
    Task<Vector> CreateEmbeddingAsync(string text);
    Task<Vector> CreateCourseDomainTagEmbeddingAsync(string courseTitle, string aimsAndObjectives, string courseContent);
    Task<Vector> CreateCourseSkillsTagEmbeddingAsync(string aimsAndObjectives, List<CILOs> cilos, string courseContent, List<TLAs> tlas, List<AssessmentMethod> assessmentMethods);
    Task<Vector> CreateCourseContentTypesTagEmbeddingAsync(string courseContent, List<TLAs> tlas, List<AssessmentMethod> assessmentMethods);
    Task<List<ChatMessage>> GenerateChatResponseAsync(List<Message> chatHistory);
}


public interface IAIProviderFactory
{
    IAIProvider GetProvider(AIProviderType providerType);
    IAIProvider GetDefaultProvider();
}

public class AIProviderFactory : IAIProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public AIProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IAIProvider GetProvider(AIProviderType providerType)
    {
        return providerType switch
        {
            AIProviderType.OpenAI => _serviceProvider.GetRequiredService<OpenAIProvider>(),
            AIProviderType.Gemini => _serviceProvider.GetRequiredService<GeminiProvider>(),
            _ => throw new ArgumentException($"Unknown AI provider type: {providerType}")
        };
    }

    public IAIProvider GetDefaultProvider()
    {
        string defaultProvider = _configuration["AI:DefaultProvider"] ?? "OpenAI";

        if (Enum.TryParse(defaultProvider, true, out AIProviderType providerType))
        {
            return GetProvider(providerType);
        }

        return GetProvider(AIProviderType.OpenAI);
    }
}
