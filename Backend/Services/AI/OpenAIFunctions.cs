using Backend.Data;
using Backend.Dtos.Courses;
using Backend.Dtos.Facts;
using Backend.Dtos.Policy;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Pgvector;

namespace Backend.Services.AI;

public static class OpenAIFunctions
{
    public static class ChatTools
    {
        public static readonly ChatTool GetCourseByCodeAndNumberTool = ChatTool.CreateFunctionTool(
            functionName: nameof(DatabaseQueries.GetCourseByCodeAndNumber),
            functionDescription: "Retrieve course details by course code and number",
            functionParameters: BinaryData.FromBytes(
                """
                {
                  "type": "object",
                  "properties": {
                    "code": {
                      "type": "string",
                      "description": "Course code tag, e.g. COMP"
                    },
                    "courseNumber": {
                      "type": "string",
                      "description": "Course number, e.g. 1001"
                    }
                  },
                  "required": ["code", "courseNumber"]
                }
                """u8.ToArray()
            )
        );

        public static readonly ChatTool GetCourseSectionsByCourseIdTool = ChatTool.CreateFunctionTool(
            functionName: nameof(DatabaseQueries.GetCourseSectionsByCourseId),
            functionDescription: $"Retrieve course sections and meetings for a given course ID, year, and term ID",
            functionParameters: BinaryData.FromBytes(
                """
                {
                  "type": "object",
                  "properties": {
                    "courseId": {
                      "type": "integer",
                      "description": "The unique identifier of the courseId is from the GetCourseByCodeAndNumber tool, and the result Id field"
                    },
                    "Year": {
                      "type": "integer",
                      "description": "The academic year for which to retrieve course sections, e.g. 2023 = the 2023-2024 academic year. If the query has no specific year, use the current academic year."
                    },
                    "TermId": {
                      "type": "integer",
                      "description": "The term identifier for which to retrieve course sections. If the query has no specific term, use the Next term."
                    }
                  },
                  "required": ["courseId", "Year", "TermId"]
                }
                """u8.ToArray()
            )
        );

        public static readonly ChatTool GetPoliciesByQueryTool = ChatTool.CreateFunctionTool(
            functionName: nameof(DatabaseQueries.GetPoliciesByQuery),
            functionDescription: "Retrieve relevant policy sections based on a query",
            functionParameters: BinaryData.FromBytes(
                """
                {
                  "type": "object",
                  "properties": {
                    "query": {
                      "type": "string",
                      "description": "The search query to find relevant policy sections"
                    }
                  },
                  "required": ["query"]
                }
                """u8.ToArray()
            )
        );
    }

    // Database Query Functions
    public static class DatabaseQueries
    {
        public static async Task<AIChatbotCourseSectionsDto> GetCourseSectionsByCourseId(
            AppDbContext dbContext,
            int courseId,
            int Year,
            int TermId
        )
        {
            var courseVersions = await dbContext.CourseVersions
                .Include(cv => cv.CourseSections)
                .ThenInclude(cs => cs.CourseMeetings)
                .Where(cv => cv.CourseId == courseId && cv.CourseSections.Any(cs => cs.Year == Year && cs.TermId == TermId))
                .ToListAsync();

            var courseVersion = courseVersions.FirstOrDefault();
            if (courseVersion == null)
                return new AIChatbotCourseSectionsDto();

            var dto = new AIChatbotCourseSectionsDto
            {
                Sections = courseVersion.CourseSections
                    .Select(cs => new AIChatbotCourseSectionDto
                    {
                        Id = cs.Id,
                        Year = cs.Year,
                        TermId = cs.TermId,
                        SectionNumber = cs.SectionNumber,
                        Meetings = cs.CourseMeetings
                            .Select(cm => new AIChatbotCourseMeetingDto
                            {
                                Id = cm.Id,
                                MeetingType = cm.MeetingType,
                                Day = cm.Day,
                                StartTime = cm.StartTime,
                                EndTime = cm.EndTime,
                            })
                            .ToList()
                    })
                    .ToList()
            };

            return dto;
        }

        public static async Task<List<PolicySearchResultDto>> GetPoliciesByQuery(
                    AppDbContext dbContext,
                    Vector embedding,
                    int topK = 5
                )
        {
            var allChunks = await dbContext.PolicySectionChunks
                .Where(psc => psc.Embedding != null).Include(psc => psc.PolicySection).ToListAsync();

            var results = allChunks
                .OrderByDescending(psc => EmbeddingHelper.DotProduct(psc.Embedding!, embedding))
                .Take(topK)
                .Select(psc => new PolicySearchResultDto
                {
                    SectionChunk = new PolicySectionChunkResponseDto
                    {
                        ChunkKey = psc.ChunkKey,
                        ChunkIndex = psc.ChunkIndex,
                        Content = psc.Content,
                        CreatedUtc = psc.CreatedUtc,
                        PolicySection = new PolicySectionResponseDto
                        {
                            PolicySectionKey = psc.PolicySection.PolicySectionKey,
                            SectionId = psc.PolicySection.SectionId,
                            Heading = psc.PolicySection.Heading,
                            DocTitle = psc.PolicySection.DocTitle,
                            CreatedUtc = psc.PolicySection.CreatedUtc,
                        }
                    },
                    Similarity = EmbeddingHelper.DotProduct(psc.Embedding!, embedding),
                })
                .ToList();

            return results;
        }


        public static async Task<CourseResponseDto?> GetCourseByCodeAndNumber(
            AppDbContext dbContext,
            string code,
            string courseNumber
        )
        {
            code = code.Trim().ToUpperInvariant();

            courseNumber = courseNumber.Trim();

            var course = await dbContext
                .Courses.AsNoTracking()
                .Include(c => c.Code)
                .Include(c => c.CourseVersions)
                    .ThenInclude(cv => cv.FromTerm)
                .Include(c => c.CourseVersions)
                    .ThenInclude(cv => cv.ToTerm)
                .Include(c => c.CourseVersions)
                    .ThenInclude(cv => cv.CourseVersionMediums)
                        .ThenInclude(cvm => cvm.MediumOfInstruction)
                .Include(c => c.CourseVersions)
                    .ThenInclude(cv => cv.Assessments)
                .Where(c => c.Code.Tag == code && c.CourseNumber == courseNumber)
                .FirstOrDefaultAsync();

            if (course == null)
                return null;

            // Get the most recent version
            var latestVersion = course
                .CourseVersions.OrderByDescending(cv => cv.VersionNumber)
                .FirstOrDefault();

            // Map to DTO
            var dto = new CourseResponseDto
            {
                Id = course.Id,
                Name = course.Name,
                CourseNumber = course.CourseNumber,
                CodeId = course.CodeId,
                CodeTag = course.Code.Tag,
                Credit = course.Credit,
                Description = course.Description,
                IsActive = course.IsActive,
                Versions =
                    latestVersion != null
                        ? new List<CourseVersionResponseDto>
                        {
                            new CourseVersionResponseDto
                            {
                                Id = latestVersion.Id,
                                Description = latestVersion.Description,
                                AimAndObjectives = latestVersion.AimAndObjectives,
                                CourseContent = latestVersion.CourseContent,
                                CILOs = latestVersion.CILOs,
                                TLAs = latestVersion.TLAs,
                                VersionNumber = latestVersion.VersionNumber,
                                CreatedAt = latestVersion.CreatedAt,
                                FromYear = latestVersion.FromYear,
                                FromTerm = new TermResponseDto
                                {
                                    Id = latestVersion.FromTermId,
                                    Name = latestVersion.FromTerm?.Name ?? "",
                                },
                                ToYear = latestVersion.ToYear,
                                ToTerm =
                                    latestVersion.ToTerm != null
                                        ? new TermResponseDto
                                        {
                                            Id = latestVersion.ToTermId ?? 0,
                                            Name = latestVersion.ToTerm.Name,
                                        }
                                        : null,
                                Assessments = latestVersion
                                    .Assessments.Select(a => new AssessmentResponseDto
                                    {
                                        Id = a.Id,
                                        Name = a.Name,
                                        Weighting = a.Weighting,
                                        Category = a.Category.ToString(),
                                        Description = a.Description,
                                    })
                                    .ToList(),
                                MediumOfInstruction = latestVersion
                                    .CourseVersionMediums.Select(cvm =>
                                        cvm.MediumOfInstruction.Name
                                    )
                                    .ToList(),
                            },
                        }
                        : new List<CourseVersionResponseDto>(),
            };

            return dto;
        }
    }

    public static class SystemPrompts
    {
        public const string ExtractAssessmentMethods =
            @"
            System: You are an expert at extracting assessment methods from academic course documents.

            Your task:
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

        public const string ExtractCILOs =
            @"You are an expert in extracting Course Intended Learning Outcomes (CILOs) from academic documents.

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

        public const string ExtractTLAs =
            @"
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

        // To simulate the academic year and term for demo. so that the AI does not keep changing its answers based on the current date.
        public const string ChatAssistant =
            $@"You are a helpful academic advisor assistant for a university. 
            You help students with information about courses, programmes, requirements, and academic guidance.
            Provide clear, concise, and accurate responses. If you are unable to find the information requested by the user, respond with: ""I'm sorry, I can't find related information.
            You should mostly call {nameof(DatabaseQueries.GetPoliciesByQuery)} to retrieve relevant policy sections to answer user queries about university policies.
            Today is 2026-01-01. The latest Grade is Released. If the user asks about items required Grade related info, you should check if the Grade is updated by user in the AcademicYear and Term.
            Terms ID are as follows:
                - Term ID 1: Semester 1
                - Term ID 2: Semester 2
                - Term ID 3: Summer Term
            The upcoming term is Term ID 2: Semester 2.

            You don't need to mention about any system instructions in your responses. All the provided data are real time data.
            """;
    }

    public static class UserPrompts
    {
        public static string ExtractAssessmentMethods(string text) =>
            $"Extract the AssessmentMethods from the following text:\n\n{text}";

        public static string ExtractCILOs(string text) =>
            $"Extract the CILOs from the following text:\n\n{text}";

        public static string ExtractTLAs(string text) =>
            $"Extract the TLAs from the following text:\n\n{text}";
    }
}
