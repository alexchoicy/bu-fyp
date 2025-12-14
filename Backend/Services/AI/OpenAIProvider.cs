using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.AI.OpenAI;
using Backend.Models;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace Backend.Services.AI;

public class OpenAIProvider : IAIProvider
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;
    private readonly ILogger<OpenAIProvider> _logger;

    public OpenAIProvider(IConfiguration configuration, ILogger<OpenAIProvider> logger)
    {
        _logger = logger;
        string endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new ArgumentNullException("AzureOpenAI:Endpoint configuration is missing");
        string apiKey = configuration["AzureOpenAI:ApiKey"]
            ?? throw new ArgumentNullException("AzureOpenAI:ApiKey configuration is missing");
        _deploymentName = "gpt-5-mini";

        _client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey), new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_12_01_Preview));
    }

    public async Task<List<AssessmentMethod>> ExtractAssessmentMethodsAsync(string text)
    {
        try
        {
            ChatClient chatClient = _client.GetChatClient(_deploymentName);

            string systemPrompt = @"
            System: You are an expert at extracting assessment methods from academic course documents.

            Your task:
            - List 3-5 concise bullets outlining your key steps.
            - From input text (e.g., section labeled ""assessmentMethods""), identify all distinct assessment methods.
            - For each method found, create a JSON object with four required fields.
            - Assign a ""category"" by analyzing only the method's name and description, and choose strictly from: ""Examination"", ""Assignment"", ""Project"", ""GroupProject"", ""SoloProject"", ""Presentation"", ""Participation"", or ""Other"". If the project type is specified as group or solo, use ""GroupProject"" or ""SoloProject""; if unspecified, use ""Project"".
            - Use only numeric weighting values; if not specified or unclear, set weighting to 0.
            - Output only a valid JSON array of AssessmentMethods objects (no additional commentary, markup, or wrapping).

            Each AssessmentMethod JSON object must include exactly these four fields:
            - ""name"": string — the exact assessment method name, copied verbatim from the input (do not summarize or paraphrase).
            - ""weighting"": number — purely numeric value (e.g., ""20%"" → 20). Use 0 if unavailable.
            - ""category"": string — one of: ""Examination"", ""Assignment"", ""Project"", ""GroupProject"", ""SoloProject"", ""Presentation"", ""Participation"", ""Other"" as determined above.
            - ""description"": string — extract exactly as plain text from the input (do not summarize, paraphrase, or use markup).

            Other instructions:
            - Ignore CILO codes unless they appear within a description.
            - Preserve the order of assessment methods as they appear in the source.
            - Return [] if no assessment methods are found.

            Output rules:
            - Output must be a syntactically-valid JSON array containing only AssessmentMethods objects with the specified four fields.
            - Do not add, remove, merge, or split assessment items beyond those presented as distinct in the input.
            - Assessment method names and descriptions must be copied exactly as in the input text; do not infer, normalize, or rephrase them.
            - If the method's description is vague or ambiguous, still copy the name and description exactly and set weighting to 0 if not clearly stated.
            ";
            string userPrompt = $"Extract the AssessmentMethods from the following text:\n\n{text}";

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            Console.WriteLine(content);

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());


            List<AssessmentMethod>? assessmentMethods = JsonSerializer.Deserialize<List<AssessmentMethod>>(content, options);

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

            //optimized by COOL COOL OPENAI OPTIMIZE PROMPT
            string systemPrompt = @"You are an expert in extracting Course Intended Learning Outcomes (CILOs) from academic documents.

            Your task:
            - Begin with a concise checklist (3-7 bullets) of what you will do; keep items conceptual, not implementation-level.
            - Extract all CILOs from the provided text and return them as a JSON array.
            - Each CILO in the array must include:
            - code: The CILO identifier (e.g., 'CILO1', 'CILO2', etc.). If the text contains no explicit code for an outcome, assign the next available sequential code in the order found (do not skip numbers, even if original codes are missing or are nonsequential).
            - description: The complete text of the outcome, concatenated as plain text without any markup. If a description spans multiple lines, merge them into a single string.

            Additional requirements:
            - If no CILOs are found, return an empty array: [].
            - If the same CILO code appears more than once, include each occurrence as a separate entry, preserving the order found in the text.
            - For outcomes formatted like CILOs but missing a code, assign the next available sequential code (e.g., 'CILO1', 'CILO2', etc.).

            Output constraints:
            - Return ONLY a valid JSON array of the following objects:
            - ""code"": The assigned or extracted CILO code.
            - ""description"": The outcome's plain text description.
            - No additional text or markdown should be returned.

            After extracting and formatting the CILOs, validate that the output is a syntactically correct JSON array of objects with the required fields before finalizing your response.

            Example output:
            [
            {""code"": ""CILO1"", ""description"": ""Describe the fundamental concepts of biology.""},
            {""code"": ""CILO2"", ""description"": ""Apply critical thinking to scientific problems.""}
            ]
            ";


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

            string userPrompt = $"Extract the CILOs from the following text:\n\n{text}";

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            List<CILOs>? cilos = JsonSerializer.Deserialize<List<CILOs>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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

            string systemPrompt = @"
                You need to extract Teaching and Learning Activities (TLAs) and their alignment to Course Intended Learning Outcomes (CILOs) from academic documentation.

                Checklist (conceptual):
                - Find all TLAs in the text.
                - Match each TLA to its CILO index/indices.
                - Return results as a JSON array of objects.
                - Ignore entries missing or with incorrect CILO–TLA matches.
                - Keep original order from the source.

                Guidelines:
                - CILO indices are lines of comma-separated numbers (e.g., ""1,2,3""). If indices span lines, join until the full list is captured.
                - Each index list applies to the TLA on the next line(s), concatenating description lines if needed (preserve line breaks).
                - Skip headers, labels, or unrelated lines.
                - Store CILO indices as a ""code"" array of strings.
                - If a CILO index doesn't directly precede a TLA, skip the TLA.
                - Omit pairs with missing or malformed indices/descriptions.
                - Use source wording for TLA descriptions, preserving line breaks from the original text.
                - Validate output as a proper JSON array; self-correct if not valid.
                - If no valid TLAs are found, return: [].

                Output only the JSON array, each object:
                - ""code"": array of CILO indices as strings
                - ""description"": TLA description (original wording, line breaks kept)

                Example:
                [
                {""code"": [""1"", ""2""], ""description"": ""Lecture""},
                {""code"": [""1"", ""2"", ""3"", ""4"", ""5""], ""description"": ""Case Study""}
                ]
            ";

            string userPrompt = $"Extract the TLAs from the following text:\n\n{text}";

            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages);
            string content = response.Value.Content[0].Text;

            List<TLAs>? tlas = JsonSerializer.Deserialize<List<TLAs>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

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
    
    
    public async Task<float[]> CreateEmbeddingAsync(string text)
    {
        try
        {
            EmbeddingClient embeddingClient = _client.GetEmbeddingClient("text-embedding-3-large");
            OpenAIEmbedding embedding = await embeddingClient.GenerateEmbeddingAsync(text);
            ReadOnlyMemory<float> vector = embedding.ToFloats();
            return vector.ToArray();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while creating embedding");
            throw;
        }
    }
}
