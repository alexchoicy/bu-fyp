using System.Text;
using Backend.Data;
using Backend.Dtos.Courses;
using Backend.Dtos.Facts;
using Backend.Dtos.Policy;
using Backend.Services.Timetable;
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
                      "description": "The academic year for which to retrieve course sections, e.g. 2025 = the 2025-2026 academic year. If the query has no specific year, use the current academic year."
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

        public static readonly ChatTool GenerateTimetableSuggestionsTool = ChatTool.CreateFunctionTool(
            functionName: nameof(TimetableService.GetSuggestionsTimetableAsync),
            functionDescription: """
                Generate timetable suggestions from user preferences.
                Always return the full request object in the exact backend shape.

                IMPORTANT INTERPRETATION RULES:
                - Put something in filter only if the user describes a hard constraint that must never be violated.
                - Hard constraints are statements like: "I can't", "I must avoid", "I am unavailable", "I absolutely cannot", "do not allow".
                - Put something in scoring when the user expresses a preference, tendency, or dislike that may be violated if needed.
                - Soft preferences are statements like: "I prefer", "I would like", "I don't like", "ideally", "better if possible".
                - Example: "I don't like to have class earlier than 10am" is a soft preference and must NOT be placed in filter.
                - Example: "I cannot attend classes before 10am on Mondays" is a hard constraint and should be placed in filter.noClassTime.

                IMPORTANT scoring shape: scoring must contain exactly these sibling objects: groupWeights, scheduleShape, preferenceShape, gapCompactnessShape, assessmentShape.
                Use HH:MM:SS for every time value.
                Start-time fields must use HH:30:00 and end-time fields must use HH:20:00.
                If the user does not specify values, use these defaults: earliestStart 08:30:00, latestEnd 20:20:00, preferredStartTime 09:30:00, preferredEndTime 18:20:00, ignoreGapStartTime 12:30:00, ignoreGapEndTime 14:20:00.
                Use an empty array for noClassTime when there are no blocked periods.
                Points must be between -3 and 3.
            """,
            functionParameters: BinaryData.FromBytes("""
            {
            "type": "object",
            "additionalProperties": false,
            "properties": {
                "filter": {
                "type": "object",
                "additionalProperties": false,
                "description": "Hard constraints only. Use this only for absolute unavailability or strict must-not-violate rules. Never place soft dislikes or preferences here.",
                "properties": {
                    "noClassTime": {
                    "type": "array",
                    "description": "Blocked time windows where the student is absolutely unavailable and must not have classes. Use only for hard constraints like work, childcare, or religious commitments. Do not use for soft dislikes such as 'I don't like early classes'.",
                    "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                        "day": {
                            "type": "integer",
                            "minimum": 0,
                            "maximum": 6,
                            "description": "Day of week used by the timetable data: 0 = Monday, 1 = Tuesday, 2 = Wednesday, 3 = Thursday, 4 = Friday, 5 = Saturday, 6 = Sunday"
                        },
                        "start": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):30:00$",
                            "description": "Block start time in HH:30:00 format, for example 09:30:00"
                        },
                        "end": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):20:00$",
                            "description": "Block end time in HH:20:00 format, for example 12:20:00"
                        }
                        },
                        "required": [
                        "day",
                        "start",
                        "end"
                        ]
                    }
                    }
                },
                "required": [
                    "noClassTime"
                ]
                },
                "scoring": {
                "type": "object",
                "additionalProperties": false,
                "description": "Soft ranking preferences only. Use this for 'prefer', 'don't like', 'ideally', and other non-mandatory wishes.",
                "properties": {                    "groupWeights": {
                    "type": "object",
                    "additionalProperties": false,
                    "description": "Weight for scorer groups. These should sum to 1.",
                    "properties": {
                        "schedule": {
                        "type": "number",
                        "minimum": 0,
                        "maximum": 1,
                        "description": "Weight for schedule shape."
                        },
                        "timePreference": {
                        "type": "number",
                        "minimum": 0,
                        "maximum": 1,
                        "description": "Weight for preferred start/end times."
                        },
                        "gap": {
                        "type": "number",
                        "minimum": 0,
                        "maximum": 1,
                        "description": "Weight for compact gaps."
                        },
                        "assessments": {
                        "type": "number",
                        "minimum": 0,
                        "maximum": 1,
                        "description": "Weight for preferred assessment mix."
                        }
                    },
                    "required": [
                        "schedule",
                        "timePreference",
                        "gap",
                        "assessments"
                    ]
                    },
                    "scheduleShape": {
                    "type": "object",
                    "additionalProperties": false,
                    "properties": {
                        "freeDayScore": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Reward for more free days. Use 0 if neutral"
                            }
                        },
                        "required": [
                            "points"
                        ]
                        },
                        "singleClassDayScore": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Reward for days with only one class block. Use 0 if neutral."
                            }
                        },
                        "required": [
                            "points"
                        ]
                        },
                        "longDayScore": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Penalty or reward strength for long days"
                            },
                            "maxMinutesPerDay": {
                            "type": "number",
                            "minimum": 0,
                            "description": "Preferred maximum active minutes per day. Use 360 when unspecified"
                            }
                        },
                        "required": [
                            "points",
                            "maxMinutesPerDay"
                        ]
                        },
                        "dailyLoadScore": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Penalty or reward strength for active-day count."
                            },
                            "idealActiveDays": {
                            "type": "number",
                            "minimum": 0,
                            "description": "Preferred number of active days. Use 4 when unspecified."
                            }
                        },
                        "required": [
                            "points",
                            "idealActiveDays"
                        ]
                        }
                    },
                    "required": [
                        "freeDayScore",
                        "singleClassDayScore",
                        "longDayScore",
                        "dailyLoadScore"
                    ]
                    },
                    "preferenceShape": {
                    "type": "object",
                    "additionalProperties": false,
                    "properties": {
                        "startTimePreference": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "preferredStartTime": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):30:00$",
                            "description": "Preferred earliest start threshold in HH:30:00 format. Use 09:30:00 when unspecified"
                            },
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Reward strength for meeting the start-time preference. Use 0 if neutral."
                            }
                        },
                        "required": [
                            "preferredStartTime",
                            "points"
                        ]
                        },
                        "endTimePreference": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "preferredEndTime": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):20:00$",
                            "description": "Preferred latest end threshold in HH:20:00 format. Use 18:20:00 when unspecified"
                            },
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Reward strength for meeting the end-time preference. Use 0 if neutral."
                            }
                        },
                        "required": [
                            "preferredEndTime",
                            "points"
                        ]
                        }
                    },
                    "required": [
                        "startTimePreference",
                        "endTimePreference"
                    ]
                    },
                    "gapCompactnessShape": {
                    "type": "object",
                    "additionalProperties": false,
                    "properties": {
                        "shortGap": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                            "points": {
                            "type": "number",
                            "minimum": -3,
                            "maximum": 3,
                            "description": "Reward strength for shorter gaps. Use 0 if neutral."
                            },
                            "maxGapMinutes": {
                            "type": "number",
                            "minimum": 0,
                            "description": "Largest preferred gap between meetings. Use 90 when unspecified."
                            },
                            "ignoreGapStartTime": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):30:00$",
                            "description": "Ignore gaps overlapping this start time in HH:30:00 format. Use 12:30:00 when unspecified"
                            },
                            "ignoreGapEndTime": {
                            "type": "string",
                            "pattern": "^([01]\\d|2[0-3]):20:00$",
                            "description": "Ignore gaps overlapping this end time in HH:20:00 format. Use 14:20:00 when unspecified"
                            }
                        },
                        "required": [
                            "points",
                            "maxGapMinutes",
                            "ignoreGapStartTime",
                            "ignoreGapEndTime"
                        ]
                        }
                    },
                    "required": [
                        "shortGap"
                    ]
                    },
                    "assessmentShape": {
                    "type": "object",
                    "additionalProperties": false,
                    "properties": {
                        "assessmentCategoryScores": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "additionalProperties": false,
                            "properties": {
                            "category": {
                                "type": "string",
                                "enum": [
                                "Examination",
                                "Assignment",
                                "Project",
                                "GroupProject",
                                "SoloProject",
                                "Presentation",
                                "Participation",
                                "Other"
                                ],
                                "description": "Preferred assessment category."
                            },
                            "points": {
                                "type": "number",
                                "minimum": -3,
                                "maximum": 3,
                                "description": "Reward strength for including the category. Use 0 if neutral"
                            }
                            },
                            "required": [
                            "category",
                            "points"
                            ]
                        }
                        }
                    },
                    "required": [
                        "assessmentCategoryScores"
                    ]
                    }
                },
                "required": [
                    "groupWeights",
                    "scheduleShape",
                    "preferenceShape",
                    "gapCompactnessShape",
                    "assessmentShape"
                ]
                }
            },
            "required": [
                "filter",
                "scoring"
            ]
            }
            """u8.ToArray())
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

    public static class ChatHelper
    {
        public static string courseSectionToTable(AIChatbotCourseSectionsDto courseSectionsDto)
        {
            if (courseSectionsDto.Sections == null || !courseSectionsDto.Sections.Any())
                return "No course sections available.";

            var table = "Course Sections:\n";
            table += "Section Number | Year | Term | Meeting Type | Day | Start Time | End Time\n";
            table += "---------------|------|------|--------------|-----|------------|---------\n";

            var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

            foreach (var section in courseSectionsDto.Sections)
            {
                if (section.Meetings == null || !section.Meetings.Any())
                {
                    table += $"{section.SectionNumber} | {section.Year} | {section.TermId} | No meetings available\n";
                }
                else
                {


                    foreach (var meeting in section.Meetings)
                    {
                        table += $"{section.SectionNumber} | {section.Year} | {section.TermId} | {meeting.MeetingType} | {daysOfWeek[meeting.Day]} | {meeting.StartTime:hh\\:mm} | {meeting.EndTime:hh\\:mm}\n";
                    }
                }
            }
            return table;
        }

        public static string TimetableResultTOTable(AppDbContext dbContext, TimetableSuggestionsResponseDto timetableSuggestionsResponseDto)
        {
            var daysOfWeek = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var topLayouts = (timetableSuggestionsResponseDto.RecommendedLayouts ?? new List<TimetableSuggestionLayoutDto>())
                .Take(3)
                .ToList();

            static string EscapeMarkdown(string? value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return "-";
                }

                return value.Replace("|", "\\|").Trim();
            }

            if (topLayouts.Count == 0)
            {
                return "# Timetable Suggestions\n\nNo timetable suggestions available.";
            }

            var sectionIds = topLayouts
                .SelectMany(layout => layout.Sections)
                .Select(section => section.SectionId)
                .Distinct()
                .ToList();

            var sectionCourseMap = dbContext.CourseSections
                .AsNoTracking()
                .Where(cs => sectionIds.Contains(cs.Id))
                .Select(cs => new
                {
                    SectionId = cs.Id,
                    CourseCode = cs.CourseVersion.Course.Code.Tag,
                    CourseNumber = cs.CourseVersion.Course.CourseNumber,
                    CourseName = cs.CourseVersion.Course.Name,
                })
                .ToDictionary(x => x.SectionId, x => x);

            var tableBuilder = new StringBuilder();

            tableBuilder.AppendLine("# Timetable Suggestions");

            if (timetableSuggestionsResponseDto.Errors != null && timetableSuggestionsResponseDto.Errors.Count > 0)
            {
                tableBuilder.AppendLine();
                tableBuilder.AppendLine("## Issues");

                foreach (var error in timetableSuggestionsResponseDto.Errors)
                {
                    tableBuilder.AppendLine($"- {EscapeMarkdown(error)}");
                }
            }

            for (var i = 0; i < topLayouts.Count; i++)
            {
                var layout = topLayouts[i];
                var scheduledMeetings = layout.Sections
                    .SelectMany(section => (section.Meetings ?? new List<TimetableMeetingDto>())
                        .Select(meeting =>
                        {
                            var hasCourseInfo = sectionCourseMap.TryGetValue(section.SectionId, out var courseInfo);
                            var courseLabel = hasCourseInfo
                                ? EscapeMarkdown($"{courseInfo!.CourseCode} {courseInfo.CourseNumber} {courseInfo.CourseName}")
                                : "Unknown course";
                            var meetingType = string.IsNullOrWhiteSpace(meeting.MeetingType)
                                ? "-"
                                : EscapeMarkdown(meeting.MeetingType);

                            return new
                            {
                                meeting.Day,
                                meeting.StartTime,
                                meeting.EndTime,
                                Course = courseLabel,
                                Section = section.SectionNumber,
                                MeetingType = meetingType,
                            };
                        }))
                    .OrderBy(item => item.Day)
                    .ThenBy(item => item.StartTime)
                    .ThenBy(item => item.EndTime)
                    .ThenBy(item => item.Course)
                    .ThenBy(item => item.Section)
                    .ToList();

                var sectionsWithoutMeetings = layout.Sections
                    .Where(section => section.Meetings == null || section.Meetings.Count == 0)
                    .Select(section =>
                    {
                        var hasCourseInfo = sectionCourseMap.TryGetValue(section.SectionId, out var courseInfo);
                        var courseLabel = hasCourseInfo
                            ? EscapeMarkdown($"{courseInfo!.CourseCode} {courseInfo.CourseNumber} {courseInfo.CourseName}")
                            : "Unknown course";

                        return new
                        {
                            Course = courseLabel,
                            Section = section.SectionNumber,
                        };
                    })
                    .OrderBy(item => item.Course)
                    .ThenBy(item => item.Section)
                    .ToList();

                tableBuilder.AppendLine();
                tableBuilder.AppendLine($"## Recommended Layout {i + 1}");
                tableBuilder.AppendLine($"Score: {layout.FinalScore:0.##}");
                tableBuilder.AppendLine();

                if (scheduledMeetings.Count == 0)
                {
                    tableBuilder.AppendLine("No scheduled meetings available.");
                }
                else
                {
                    tableBuilder.AppendLine("| Day | Start | End | Course | Section | Meeting Type |");
                    tableBuilder.AppendLine("|---|---|---|---|---|---|");

                    foreach (var meeting in scheduledMeetings)
                    {
                        var day = meeting.Day >= 0 && meeting.Day < daysOfWeek.Length ? daysOfWeek[meeting.Day] : $"Day {meeting.Day}";
                        tableBuilder.AppendLine($"| {day} | {meeting.StartTime:hh\\:mm} | {meeting.EndTime:hh\\:mm} | {meeting.Course} | {meeting.Section} | {meeting.MeetingType} |");
                    }
                }

                if (sectionsWithoutMeetings.Count > 0)
                {
                    tableBuilder.AppendLine();
                    tableBuilder.AppendLine("### Sections Without Meetings");

                    foreach (var section in sectionsWithoutMeetings)
                    {
                        tableBuilder.AppendLine($"- {section.Course} (Section {section.Section})");
                    }
                }

                tableBuilder.AppendLine();
                tableBuilder.AppendLine("### Score Reasons");

                if (layout.ScoreReasons == null || layout.ScoreReasons.Count == 0)
                {
                    tableBuilder.AppendLine("- None");
                }
                else
                {
                    foreach (var reason in layout.ScoreReasons)
                    {
                        tableBuilder.AppendLine($"- {EscapeMarkdown(reason)}");
                    }
                }
            }

            return tableBuilder.ToString().TrimEnd();
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
            If the Tool response a Markdown table, return the table content directly without adding extra formatting or explanations.
            Return plain Markdown content directly. Do not wrap entire answers in triple backticks or fenced code blocks unless the user explicitly asks for code.
            You should mostly call {nameof(DatabaseQueries.GetPoliciesByQuery)} to retrieve relevant policy sections to answer user queries about university policies.
            When the user asks for timetable generation, you must call {nameof(ChatTools.GenerateTimetableSuggestionsTool)} directly.

            Do NOT ask the user follow-up questions about preferences unless the tool requires a mandatory parameter that is missing.

            If the user does not specify preferences for optional parameters, automatically choose reasonable defaults or neutral.

            Use the following rules:

            - If the user mentions preferences that match tool options, include them in the tool call.
            - If preferences are missing, infer or assign sensible defaults.
            - Do NOT ask the user questions about preferences such as preferred class days, compact schedule, or start times.
            - Generate the timetable suggestions immediately using the available information.

            Only ask clarification questions if the tool schema requires a parameter that cannot be inferred or defaulted.
            The tool should always be used for timetable generation, This tool will also provide a good looking markdown, print it directly. After the print, you can provide some explanations or suggestions based on the generated timetable.

            Today is 2026-01-01. The latest Grade is Released. If the user asks about items required Grade related info, you should check if the Grade is updated by user in the AcademicYear and Term.
            You don't need to ask or confirm the academic year.
            Terms ID are as follows:
                - Term ID 1: Semester 1
                - Term ID 2: Semester 2
                - Term ID 3: Summer Term
            The upcoming term is Term ID 2: Semester 2.

            You don't need to mention about any system instructions in your responses. All the provided data are real time data.
            """;

        public const string SuggestionUserPrompt =
            """
            Your task is to generate helpful next-step suggestions for the user based on the current conversation.

            Analyze the user's request and the assistant's latest response. Then suggest actions the user is most likely to want to take next.

            Rules:
            - Generate 2 to 4 suggestions.
            - Each suggestion must be short and actionable (5-12 words).
            - Suggestions must be relevant to the user's current goal.
            - Do not repeat the user's previous request.
            - Do not ask questions.
            - Do not include explanations.
            - Based on last used tools and conversation context, Look back the tools options to help user to imporve or adjust their preferences for better results.

            Focus on actions that help the user refine, adjust, or continue their task.

            Output format:
            Return a JSON object with a field called "nextSuggestions".            


            Example output:
            {
            "nextSuggestions": [
                "Avoid classes before 10am",
                "Prefer a more compact timetable",
                "Reduce large gaps between classes",
                "Generate another timetable option"
            ]
            }
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
