using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Backend.Data;
using Backend.Models;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Pgvector;

namespace Backend.Services.AI;

public class OpenAIProvider : IAIProvider
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<OpenAIProvider> _logger;
    private readonly AppDbContext _dbContext;

    public OpenAIProvider(
        IConfiguration configuration,
        ILogger<OpenAIProvider> logger,
        AppDbContext dbContext
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        string endpoint =
            configuration["AzureOpenAI:Endpoint"]
            ?? throw new ArgumentNullException("AzureOpenAI:Endpoint configuration is missing");
        string apiKey =
            configuration["AzureOpenAI:ApiKey"]
            ?? throw new ArgumentNullException("AzureOpenAI:ApiKey configuration is missing");

        var httpClient = new HttpClient(new LoggingHandler(logger));

        // _deploymentName = "gpt-5-mini";
        //only school API use this
        // var options = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_12_01_Preview)
        // {
        //     Transport = new HttpClientPipelineTransport(httpClient)
        // };

        _deploymentName = "gpt-5-nano";
        var options = new AzureOpenAIClientOptions()
        {
            Transport = new HttpClientPipelineTransport(httpClient),
        };

        _client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey), options);
    }

    public async Task<List<AssessmentMethod>> ExtractAssessmentMethodsAsync(string text)
    {
        try
        {
            ChatClient chatClient = _client.GetChatClient(_deploymentName);

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(OpenAIFunctions.SystemPrompts.ExtractAssessmentMethods),
                new UserChatMessage(OpenAIFunctions.UserPrompts.ExtractAssessmentMethods(text)),
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            Console.WriteLine(content);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            List<AssessmentMethod>? assessmentMethods = JsonSerializer.Deserialize<
                List<AssessmentMethod>
            >(content, options);

            return assessmentMethods ?? new List<AssessmentMethod>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Failed to parse OpenAI response as JSON");
            return new List<AssessmentMethod>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while extracting AssessmentMethods using OpenAI");
            return new List<AssessmentMethod>();
        }
    }

    public async Task<List<CILOs>> ExtractCILOsAsync(string text)
    {
        try
        {
            ChatClient chatClient = _client.GetChatClient(_deploymentName);

            //tell me if this Structured model outputs cool things work on the API

            // ChatResponseFormat responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            //     jsonSchemaFormatName: "CILOsFormat",
            //     jsonSchema: BinaryData.FromBytes("""
            //         {
            //         "type": "array",
            //         "items": {
            //             "type": "object",
            //             "properties": {
            //                 "code": { "type": "string" },
            //                 "description": { "type": "string" }
            //             },
            //             "required": ["code", "description"]
            //         }
            //         }
            //         """u8.ToArray()
            //     ),
            //     jsonSchemaIsStrict: true
            // );

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(OpenAIFunctions.SystemPrompts.ExtractCILOs),
                new UserChatMessage(OpenAIFunctions.UserPrompts.ExtractCILOs(text)),
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            List<CILOs>? cilos = JsonSerializer.Deserialize<List<CILOs>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return cilos ?? new List<CILOs>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Failed to parse OpenAI response as JSON");
            return new List<CILOs>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while extracting CILOs using OpenAI");
            return new List<CILOs>();
        }
    }

    public async Task<List<TLAs>> ExtractTLAsAsync(string text)
    {
        try
        {
            ChatClient chatClient = _client.GetChatClient(_deploymentName);

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(OpenAIFunctions.SystemPrompts.ExtractTLAs),
                new UserChatMessage(OpenAIFunctions.UserPrompts.ExtractTLAs(text)),
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            List<TLAs>? tlas = JsonSerializer.Deserialize<List<TLAs>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return tlas ?? new List<TLAs>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Failed to parse OpenAI response as JSON");
            return new List<TLAs>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while extracting TLAs using OpenAI");
            return new List<TLAs>();
        }
    }

    public async Task<Vector> CreateEmbeddingAsync(string text)
    {
        try
        {
            EmbeddingClient embeddingClient = _client.GetEmbeddingClient("text-embedding-3-large");
            OpenAIEmbedding embedding = await embeddingClient.GenerateEmbeddingAsync(text);
            ReadOnlyMemory<float> vector = embedding.ToFloats();
            return new Vector(vector);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating embedding");
            throw;
        }
    }

    public async Task<Vector> CreateCourseDomainTagEmbeddingAsync(
        string courseTitle,
        string aimsAndObjectives,
        string courseContent
    )
    {
        try
        {
            string formattedText = EmbeddingHelper.FormatCourseDataForDomainTagEmbedding(
                courseTitle,
                aimsAndObjectives,
                courseContent
            );

            return await CreateEmbeddingAsync(formattedText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating course embedding");
            throw;
        }
    }

    public async Task<Vector> CreateCourseSkillsTagEmbeddingAsync(
        string aimsAndObjectives,
        List<CILOs> cilos,
        string courseContent,
        List<TLAs> tlas,
        List<AssessmentMethod> assessmentMethods
    )
    {
        try
        {
            string formattedText = EmbeddingHelper.FormatCourseDataForSkillsTagEmbedding(
                aimsAndObjectives,
                cilos,
                courseContent,
                tlas,
                assessmentMethods
            );

            return await CreateEmbeddingAsync(formattedText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating course skills tag embedding");
            throw;
        }
    }

    public async Task<Vector> CreateCourseContentTypesTagEmbeddingAsync(
        string courseContent,
        List<TLAs> tlas,
        List<AssessmentMethod> assessmentMethods
    )
    {
        try
        {
            string formattedText = EmbeddingHelper.FormatCourseDataForContentTypesTagEmbedding(
                courseContent,
                tlas,
                assessmentMethods
            );

            return await CreateEmbeddingAsync(formattedText);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating course content types tag embedding");
            throw;
        }
    }

    public async Task<List<ChatMessage>> GenerateChatResponseAsync(List<Message> chatHistory)
    {
        try
        {
            ChatClient chatClient = _client.GetChatClient(_deploymentName);

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(OpenAIFunctions.SystemPrompts.ChatAssistant),
            };

            foreach (var msg in chatHistory)
            {
                if (msg.Role == MessageRole.User)
                {
                    messages.Add(new UserChatMessage(msg.Content));
                }
                else if (msg.Role == MessageRole.Assistant)
                {
                    messages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            ChatCompletionOptions options = new ChatCompletionOptions()
            {
                Tools = { OpenAIFunctions.ChatTools.GetCourseByCodeAndNumberTool, OpenAIFunctions.ChatTools.GetPoliciesByQueryTool },
            };

            bool requiresAction;

            do
            {
                requiresAction = false;
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
                switch (completion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            messages.Add(new AssistantChatMessage(completion));
                            break;
                        }
                    case ChatFinishReason.ToolCalls:
                        {
                            _logger.LogInformation(
                                "Model requested {ToolCount} tool call(s)",
                                completion.ToolCalls.Count
                            );

                            messages.Add(new AssistantChatMessage(completion));
                            foreach (ChatToolCall toolCall in completion.ToolCalls)
                            {
                                _logger.LogInformation(
                                    "Tool requested: {ToolName}, ToolCallId: {ToolCallId}, Arguments: {Arguments}",
                                    toolCall.FunctionName,
                                    toolCall.Id,
                                    toolCall.FunctionArguments
                                );

                                switch (toolCall.FunctionName)
                                {
                                    case nameof(
                                        OpenAIFunctions.DatabaseQueries.GetCourseByCodeAndNumber
                                    ):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(
                                                toolCall.FunctionArguments
                                            );
                                            bool hasCode = argumentsJson.RootElement.TryGetProperty(
                                                "code",
                                                out JsonElement codeElement
                                            );
                                            bool hasCourseNumber = argumentsJson.RootElement.TryGetProperty(
                                                "courseNumber",
                                                out JsonElement courseNumberElement
                                            );

                                            if (!hasCode || !hasCourseNumber)
                                            {
                                                messages.Add(
                                                    new ToolChatMessage(
                                                        toolCall.Id,
                                                        ChatMessageContentPart.CreateTextPart(
                                                            "Error: Invalid arguments provided to GetCourseByCodeAndNumber."
                                                        )
                                                    )
                                                );
                                                continue;
                                            }

                                            string? codeValue = codeElement.GetString();
                                            string? courseNumberValue = courseNumberElement.GetString();

                                            var toolResult =
                                                await OpenAIFunctions.DatabaseQueries.GetCourseByCodeAndNumber(
                                                    _dbContext,
                                                    codeValue!,
                                                    courseNumberValue!
                                                );
                                            messages.Add(
                                                new ToolChatMessage(
                                                    toolCall.Id,
                                                    ChatMessageContentPart.CreateTextPart(
                                                        JsonSerializer.Serialize(toolResult)
                                                    )
                                                )
                                            );
                                            break;
                                        }
                                    case nameof(OpenAIFunctions.DatabaseQueries.GetPoliciesByQuery):
                                        {
                                            using JsonDocument argumentsJson = JsonDocument.Parse(
                                                toolCall.FunctionArguments
                                            );
                                            bool hasQuery = argumentsJson.RootElement.TryGetProperty(
                                                "query",
                                                out JsonElement queryElement
                                            );

                                            if (!hasQuery)
                                            {
                                                messages.Add(
                                                    new ToolChatMessage(
                                                        toolCall.Id,
                                                        ChatMessageContentPart.CreateTextPart(
                                                            "Error: Invalid arguments provided to GetPoliciesByQuery."
                                                        )
                                                    )
                                                );
                                                continue;
                                            }

                                            string? queryValue = queryElement.GetString();
                                            Vector queryEmbedding = await CreateEmbeddingAsync(
                                                queryValue!
                                            );

                                            var toolResult =
                                                await OpenAIFunctions.DatabaseQueries.GetPoliciesByQuery(
                                                    _dbContext,
                                                    queryEmbedding,
                                                    topK: 2
                                                );

                                            var context = string.Join("\n\n", toolResult
                                                .OrderByDescending(r => r.Similarity)
                                                .Select(r => $"From {r.SectionChunk.PolicySection.DocTitle} - {r.SectionChunk.PolicySection.Heading}:\n{r.SectionChunk.Content}"));

                                            foreach(var result in toolResult)
                                            {
                                                _logger.LogInformation("Retrieved policy section with similarity {Similarity}: {DocTitle} - {Heading}", result.Similarity, result.SectionChunk.PolicySection.DocTitle, result.SectionChunk.PolicySection.Heading);
                                            }

                                            messages.Add(
                                                new ToolChatMessage(
                                                    toolCall.Id,
                                                    ChatMessageContentPart.CreateTextPart(
                                                        context
                                                    )
                                                )
                                            );
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException(
                                                $"Function {toolCall.FunctionName} is not implemented"
                                            );
                                        }
                                }
                            }

                            requiresAction = true;
                            break;
                        }

                    case ChatFinishReason.Length:
                        throw new NotImplementedException(
                            "Incomplete model output due to MaxTokens parameter or token limit exceeded."
                        );

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException(
                            "Omitted content due to a content filter flag."
                        );

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");
                    default:
                        throw new NotImplementedException(completion.FinishReason.ToString());
                }
            } while (requiresAction);

            _logger.LogInformation("Generated chat response successfully");
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating chat response");
            throw;
        }
    }

    private sealed class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
            : base(new HttpClientHandler())
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            string? body =
                request.Content != null
                    ? await request.Content.ReadAsStringAsync(cancellationToken)
                    : null;
            _logger.LogInformation(
                "OpenAI request {Method} {Uri}\nHeaders: {Headers}\nBody: {Body}",
                request.Method,
                request.RequestUri,
                request.Headers,
                body
            );
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
