using System.Text;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Dtos.Courses;
using Backend.Dtos.Facts;
using Backend.Models;
using Backend.Services.AI;
using Backend.Services.Facts;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Backend.Services.Courses;

public interface ICourseService
{
    Task<List<CourseResponseDto>> GetCoursesAsync();
    Task<List<CourseResponseDto>> GetCoursesAsync(string studentId);
    Task<CourseResponseDto?> GetCourseByIdAsync(int id);
    Task<List<SimpleCourseDto>> GetSimpleCoursesAsync();

    Task<PdfParseResponseDto> ProcessPdfAsync(Stream pdfStream, string fileName, long fileSize,
        AIProviderType? providerType = null);

    Task<int> CreateCourseAsync(CreateCourseDto createCourseDto);
    Task<int> CreateCourseVersionAsync(int courseId, CreateCourseVersionDto createVersionDto);

    Task<TimetableResponseDto> GetTimetableAsync(string studentId, bool excludeEnrolled = true, int? year = null,
        int? termId = null, int? courseGroupId = null, int? categoryGroupId = null);
}

public class CourseService : ICourseService
{
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly AppDbContext _context;
    private readonly IFactService _factService;

    private readonly Dictionary<string, string> sectionPatterns = new Dictionary<string, string>
    {
        { "COURSE_TITLE", @"1\.\s*COURSE TITLE[:\s]*(.+?)(?=2\.\s*COURSE CODE|$)" },
        { "COURSE_CODE", @"2\.\s*COURSE CODE[:\s]*(.+?)(?=3\.\s*NO\.\s*OF UNITS|$)" },
        { "NO_OF_UNITS", @"3\.\s*NO\.\s*OF UNITS[:\s]*(.+?)(?=4\.\s*OFFERING DEPARTMENT|$)" },
        { "OFFERING_DEPARTMENT", @"4\.\s*OFFERING DEPARTMENT[:\s]*(.+?)(?=5\.\s*PREREQUISITES|$)" },
        { "PREREQUISITES", @"5\.\s*PREREQUISITES[:\s]*(.+?)(?=6\.\s*MEDIUM OF INSTRUCTION|$)" },
        { "MEDIUM_OF_INSTRUCTION", @"6\.\s*MEDIUM OF INSTRUCTION[:\s]*(.+?)(?=7\.\s*AIMS\s*[&AND]*\s*OBJECTIVES|$)" },
        { "AIMS_OBJECTIVES", @"7\.\s*AIMS\s*[&AND]*\s*OBJECTIVES[:\s]*(.+?)(?=8\.\s*COURSE CONTENT|$)" },
        { "COURSE_CONTENT", @"8\.\s*COURSE CONTENT[:\s]*(.+?)(?=9\.\s*COURSE INTENDED LEARNING OUTCOMES|$)" },
        {
            "CILOS",
            @"9\.\s*COURSE INTENDED LEARNING OUTCOMES\s*\(?CILOs?\)?[:\s]*(.+?)(?=10\.\s*TEACHING\s*[&AND]*\s*LEARNING ACTIVITIES|$)"
        },
        {
            "TLAS",
            @"10\.\s*TEACHING\s*[&AND]*\s*LEARNING ACTIVITIES\s*\(?TLAs?\)?[:\s]*(.+?)(?=11\.\s*ASSESSMENT METHODS|$)"
        },
        { "ASSESSMENT_METHODS", @"11\.\s*ASSESSMENT METHODS\s*\(?AMs?\)?[:\s]*(.+?)$" }
    };

    private static readonly Regex courseCodeExtract = new Regex(@"\b([A-Za-z]{4})\s?(\d{4})\b");

    public CourseService(AppDbContext context, IAIProviderFactory aiProviderFactory, IFactService factService)
    {
        _context = context;
        _aiProviderFactory = aiProviderFactory;
        _factService = factService;
    }

    private CourseResponseDto MapCourseToResponseDto(Course course)
    {
        return new CourseResponseDto
        {
            Id = course.Id,
            Name = course.Name,
            CourseNumber = course.CourseNumber,
            CodeId = course.CodeId,
            CodeTag = course.Code.Tag,
            Credit = course.Credit,
            IsActive = course.IsActive,
            Description = course.Description,
            Departments = course.CourseDepartments
                .Select(cd => cd.Department.Name)
                .ToList(),
            Versions = course.CourseVersions
                .OrderByDescending(cv => cv.VersionNumber)
                .Select(cv => new CourseVersionResponseDto
                {
                    Id = cv.Id,
                    Description = cv.Description,
                    AimAndObjectives = cv.AimAndObjectives,
                    CourseContent = cv.CourseContent,
                    CILOs = cv.CILOs,
                    TLAs = cv.TLAs,
                    VersionNumber = cv.VersionNumber,
                    CreatedAt = cv.CreatedAt,
                    FromYear = cv.FromYear,
                    FromTerm = new TermResponseDto
                    {
                        Id = cv.FromTerm!.Id,
                        Name = cv.FromTerm.Name
                    },
                    ToYear = cv.ToYear,
                    ToTerm = cv.ToTerm != null
                        ? new TermResponseDto
                        {
                            Id = cv.ToTerm.Id,
                            Name = cv.ToTerm.Name
                        }
                        : null,
                    MediumOfInstruction = cv.CourseVersionMediums
                        .Select(cvm => cvm.MediumOfInstruction.Name)
                        .ToList(),
                    Assessments = cv.Assessments
                        .Select(a => new AssessmentResponseDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Weighting = a.Weighting,
                            Category = a.Category.ToString(),
                            Description = a.Description
                        })
                        .ToList(),
                    PreRequisites = cv.Prerequisites
                        .Select(pr => new SimpleCourseDto
                        {
                            Id = pr.RequiredCourseVersion.Course.Id,
                            Name = pr.RequiredCourseVersion.Course.Name,
                            CourseNumber = pr.RequiredCourseVersion.Course.CourseNumber,
                            CodeId = pr.RequiredCourseVersion.Course.CodeId,
                            CodeTag = pr.RequiredCourseVersion.Course.Code.Tag,
                            MostRecentVersion = null
                        })
                        .ToList(),
                    AntiRequisites = cv.AntiRequisites
                        .Select(ar => new SimpleCourseDto
                        {
                            Id = ar.ExcludedCourseVersion.Course.Id,
                            Name = ar.ExcludedCourseVersion.Course.Name,
                            CourseNumber = ar.ExcludedCourseVersion.Course.CourseNumber,
                            CodeId = ar.ExcludedCourseVersion.Course.CodeId,
                            CodeTag = ar.ExcludedCourseVersion.Course.Code.Tag,
                            MostRecentVersion = null
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<List<CourseResponseDto>> GetCoursesAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.CourseVersionMediums)
            .ThenInclude(cvm => cvm.MediumOfInstruction)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Assessments)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.FromTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.ToTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Prerequisites)
            .ThenInclude(pr => pr.RequiredCourseVersion)
            .ThenInclude(rcv => rcv.Course)
            .ThenInclude(rc => rc.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.AntiRequisites)
            .ThenInclude(ar => ar.ExcludedCourseVersion)
            .ThenInclude(ecv => ecv.Course)
            .ThenInclude(ec => ec.Code)
            .Include(c => c.CourseDepartments)
            .ThenInclude(cd => cd.Department)
            .ToListAsync();

        return courses.Select(c => MapCourseToResponseDto(c)).ToList();
    }

    public async Task<List<CourseResponseDto>> GetCoursesAsync(string studentId)
    {
        // Get the list of course IDs the student is already enrolled in
        var enrolledCourseIds = await _context.StudentCourses
            .Where(sc => sc.StudentId == studentId)
            .Select(sc => sc.CourseId)
            .ToListAsync();

        var courses = await _context.Courses
            .Where(c => !enrolledCourseIds.Contains(c.Id))
            .Include(c => c.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.CourseVersionMediums)
            .ThenInclude(cvm => cvm.MediumOfInstruction)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Assessments)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.FromTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.ToTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Prerequisites)
            .ThenInclude(pr => pr.RequiredCourseVersion)
            .ThenInclude(rcv => rcv.Course)
            .ThenInclude(rc => rc.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.AntiRequisites)
            .ThenInclude(ar => ar.ExcludedCourseVersion)
            .ThenInclude(ecv => ecv.Course)
            .ThenInclude(ec => ec.Code)
            .Include(c => c.CourseDepartments)
            .ThenInclude(cd => cd.Department)
            .ToListAsync();


        return courses.Select(c => MapCourseToResponseDto(c)).ToList();
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.CourseVersionMediums)
            .ThenInclude(cvm => cvm.MediumOfInstruction)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Assessments)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.FromTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.ToTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Prerequisites)
            .ThenInclude(pr => pr.RequiredCourseVersion)
            .ThenInclude(rcv => rcv.Course)
            .ThenInclude(rc => rc.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.AntiRequisites)
            .ThenInclude(ar => ar.ExcludedCourseVersion)
            .ThenInclude(ecv => ecv.Course)
            .ThenInclude(ec => ec.Code)
            .Include(c => c.CourseDepartments)
            .ThenInclude(cd => cd.Department)
            .FirstOrDefaultAsync(c => c.Id == id);

        return course != null ? MapCourseToResponseDto(course) : null;
    }

    public async Task<List<SimpleCourseDto>> GetSimpleCoursesAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.CourseVersionMediums)
            .ThenInclude(cvm => cvm.MediumOfInstruction)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Assessments)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.FromTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.ToTerm)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.Prerequisites)
            .ThenInclude(pr => pr.RequiredCourseVersion)
            .ThenInclude(rcv => rcv.Course)
            .ThenInclude(rc => rc.Code)
            .Include(c => c.CourseVersions)
            .ThenInclude(cv => cv.AntiRequisites)
            .ThenInclude(ar => ar.ExcludedCourseVersion)
            .ThenInclude(ecv => ecv.Course)
            .ThenInclude(ec => ec.Code)
            .ToListAsync();

        return courses.Select(c => new SimpleCourseDto
        {
            Id = c.Id,
            Name = c.Name,
            CourseNumber = c.CourseNumber,
            CodeId = c.CodeId,
            CodeTag = c.Code.Tag,
            MostRecentVersion = c.CourseVersions
                .OrderByDescending(cv => cv.VersionNumber)
                .Select(cv => new CourseVersionResponseDto
                {
                    Id = cv.Id,
                    Description = cv.Description,
                    AimAndObjectives = cv.AimAndObjectives,
                    CourseContent = cv.CourseContent,
                    CILOs = cv.CILOs,
                    TLAs = cv.TLAs,
                    VersionNumber = cv.VersionNumber,
                    CreatedAt = cv.CreatedAt,
                    FromYear = cv.FromYear,
                    FromTerm = new TermResponseDto
                    {
                        Id = cv.FromTerm!.Id,
                        Name = cv.FromTerm.Name
                    },
                    ToYear = cv.ToYear,
                    ToTerm = cv.ToTerm != null
                        ? new TermResponseDto
                        {
                            Id = cv.ToTerm.Id,
                            Name = cv.ToTerm.Name
                        }
                        : null,
                    MediumOfInstruction = cv.CourseVersionMediums
                        .Select(cvm => cvm.MediumOfInstruction.Name)
                        .ToList(),
                    Assessments = cv.Assessments
                        .Select(a => new AssessmentResponseDto
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Weighting = a.Weighting,
                            Category = a.Category.ToString(),
                            Description = a.Description
                        })
                        .ToList(),
                    PreRequisites = cv.Prerequisites
                        .Select(pr => new SimpleCourseDto
                        {
                            Id = pr.RequiredCourseVersion.Course.Id,
                            Name = pr.RequiredCourseVersion.Course.Name,
                            CourseNumber = pr.RequiredCourseVersion.Course.CourseNumber,
                            CodeId = pr.RequiredCourseVersion.Course.CodeId,
                            CodeTag = pr.RequiredCourseVersion.Course.Code.Tag,
                            MostRecentVersion = null
                        })
                        .ToList(),
                    AntiRequisites = cv.AntiRequisites
                        .Select(ar => new SimpleCourseDto
                        {
                            Id = ar.ExcludedCourseVersion.Course.Id,
                            Name = ar.ExcludedCourseVersion.Course.Name,
                            CourseNumber = ar.ExcludedCourseVersion.Course.CourseNumber,
                            CodeId = ar.ExcludedCourseVersion.Course.CodeId,
                            CodeTag = ar.ExcludedCourseVersion.Course.Code.Tag,
                            MostRecentVersion = null
                        })
                        .ToList()
                })
                .FirstOrDefault()
        }).ToList();
    }


    private (string, string) ExtractCourseCodeAndNumber(string courseCodeRaw)
    {
        Match match = courseCodeExtract.Match(courseCodeRaw);
        if (match.Success && match.Groups.Count == 3)
        {
            return (match.Groups[1].Value.ToUpper(), match.Groups[2].Value);
        }

        return (string.Empty, string.Empty);
    }

    private List<CourseCodeDto> ExtractMultipleCourses(string prerequisitesText)
    {
        var courses = new List<CourseCodeDto>();

        if (string.IsNullOrWhiteSpace(prerequisitesText))
        {
            return courses;
        }

        string[] lines = prerequisitesText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedLine))
            {
                (string courseCode, string courseNumber) = ExtractCourseCodeAndNumber(trimmedLine);
                if (!string.IsNullOrEmpty(courseCode) && !string.IsNullOrEmpty(courseNumber))
                {
                    courses.Add(new CourseCodeDto
                    {
                        CourseCode = courseCode,
                        CourseCodeNumber = courseNumber
                    });
                }
            }
        }

        return courses;
    }

    private ParsedSectionsDto ParseCourseContent(string fullText)
    {
        Dictionary<string, string> sections = new Dictionary<string, string>();

        foreach ((string key, string pattern) in sectionPatterns)
        {
            Match match = Regex.Match(fullText, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                sections[key] = match.Groups[1].Value.Trim();
            }
            else
            {
                sections[key] = "";
            }
        }

        (string courseCode, string courseNumber) = ExtractCourseCodeAndNumber(sections["COURSE_CODE"]);


        return new ParsedSectionsDto
        {
            CourseTitle = sections["COURSE_TITLE"],
            CourseCode = courseCode,
            CourseCodeNumber = courseNumber,
            NoOfUnits = sections["NO_OF_UNITS"],
            OfferingDepartment = sections["OFFERING_DEPARTMENT"],
            Prerequisites = ExtractMultipleCourses(sections["PREREQUISITES"]),
            MediumOfInstruction = sections["MEDIUM_OF_INSTRUCTION"],
            AimsObjectives = sections["AIMS_OBJECTIVES"],
            CourseContent = sections["COURSE_CONTENT"],
            CilosRaw = sections["CILOS"],
            TlasRaw = sections["TLAS"],
            AssessmentMethodsRaw = sections["ASSESSMENT_METHODS"]
        };
    }

    // this bro is to extract text from pdf and parse it into sections
    public async Task<PdfParseResponseDto> ProcessPdfAsync(Stream pdfStream, string fileName, long fileSize,
        AIProviderType? providerType = null)
    {
        using PdfDocument document = PdfDocument.Open(pdfStream);
        StringBuilder extractedText = new StringBuilder();

        foreach (Page page in document.GetPages())
        {
            IEnumerable<Word> words = NearestNeighbourWordExtractor.Instance.GetWords(page.Letters);
            IReadOnlyList<TextBlock> blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
            foreach (TextBlock block in blocks)
            {
                extractedText.AppendLine(block.Text);
                extractedText.AppendLine();
            }
        }

        string fullText = extractedText.ToString();
        ParsedSectionsDto parsedSections = ParseCourseContent(fullText);

        await ExtractDataWithAI(parsedSections, providerType);

        return new PdfParseResponseDto
        {
            Message = "PDF uploaded and processed successfully",
            Filename = fileName,
            Size = fileSize,
            Pages = document.NumberOfPages,
            ExtractedText = fullText,
            ParsedSections = parsedSections
        };
    }

    //TODO: I think it is possible to extract without AI, try later
    //Tabula-sharp
    public async Task ExtractDataWithAI(ParsedSectionsDto parsedCourseData, AIProviderType? providerType = null)
    {
        IAIProvider provider = providerType.HasValue
            ? _aiProviderFactory.GetProvider(providerType.Value)
            : ((AIProviderFactory)_aiProviderFactory).GetDefaultProvider();

        Task<List<CILOs>>? cilosTask = !string.IsNullOrWhiteSpace(parsedCourseData.CilosRaw)
            ? provider.ExtractCILOsAsync(parsedCourseData.CilosRaw)
            : Task.FromResult(new List<CILOs>());

        Task<List<AssessmentMethod>>? assessmentTask = !string.IsNullOrWhiteSpace(parsedCourseData.AssessmentMethodsRaw)
            ? provider.ExtractAssessmentMethodsAsync(parsedCourseData.AssessmentMethodsRaw)
            : Task.FromResult(new List<AssessmentMethod>());

        Task<List<TLAs>>? tlasTask = !string.IsNullOrWhiteSpace(parsedCourseData.TlasRaw)
            ? provider.ExtractTLAsAsync(parsedCourseData.TlasRaw)
            : Task.FromResult(new List<TLAs>());

        await Task.WhenAll(cilosTask, assessmentTask, tlasTask);

        parsedCourseData.CILOs = await cilosTask;
        parsedCourseData.AssessmentMethods = await assessmentTask;
        parsedCourseData.TLAs = await tlasTask;
    }

    public async Task<int> CreateCourseAsync(CreateCourseDto createCourseDto)
    {
        var code = await _context.Codes.FindAsync(createCourseDto.CodeId);
        if (code == null)
        {
            throw new ArgumentException($"Code with ID {createCourseDto.CodeId} not found");
        }

        Course? existingCourse = await _context.Courses
            .FirstOrDefaultAsync(c =>
                c.CodeId == createCourseDto.CodeId && c.CourseNumber == createCourseDto.CourseNumber);
        if (existingCourse != null)
        {
            throw new InvalidOperationException(
                $"Course with code {code.Tag} and number {createCourseDto.CourseNumber} already exists");
        }

        Course course = new Course
        {
            Name = createCourseDto.Name,
            CourseNumber = createCourseDto.CourseNumber,
            CodeId = createCourseDto.CodeId,
            Credit = createCourseDto.Credit,
            Description = createCourseDto.Description,
            IsActive = createCourseDto.IsActive
        };

        _context.Courses.Add(course);

        if (createCourseDto.DepartmentIds != null && createCourseDto.DepartmentIds.Count > 0)
        {
            var distinctDeptIds = createCourseDto.DepartmentIds.Distinct().ToList();

            var existingDeptIds = await _context.Departments
                .Where(d => distinctDeptIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();

            var missingDepts = distinctDeptIds.Except(existingDeptIds).ToList();
            if (missingDepts.Any())
            {
                throw new ArgumentException($"Department(s) with ID(s) {string.Join(',', missingDepts)} not found");
            }

            foreach (var deptId in distinctDeptIds)
            {
                var courseDepartment = new CourseDepartment
                {
                    Course = course,
                    DepartmentId = deptId
                };
                _context.CourseDepartments.Add(courseDepartment);
            }
        }

        if (createCourseDto.GroupIds != null && createCourseDto.GroupIds.Count > 0)
        {
            var distinctIds = createCourseDto.GroupIds.Distinct().ToList();

            var existingGroupIds = await _context.CourseGroups
                .Where(g => distinctIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            var missing = distinctIds.Except(existingGroupIds).ToList();
            if (missing.Any())
            {
                throw new ArgumentException($"Course group(s) with ID(s) {string.Join(',', missing)} not found");
            }

            foreach (var gid in distinctIds)
            {
                var groupCourse = new GroupCourse
                {
                    GroupId = gid,
                    Course = course
                };
                _context.GroupCourses.Add(groupCourse);
            }
        }

        await _context.SaveChangesAsync();
        return course.Id;
    }

    public async Task<int> CreateCourseVersionAsync(int courseId, CreateCourseVersionDto createVersionDto)
    {
        Course? course = await _context.Courses
            .Include(c => c.CourseVersions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            throw new ArgumentException($"Course with ID {courseId} not found");
        }

        Term? fromTerm = await _context.Terms.FindAsync(createVersionDto.FromTermId);
        if (fromTerm == null)
        {
            throw new ArgumentException($"Term with ID {createVersionDto.FromTermId} not found");
        }

        if (createVersionDto.ToTermId.HasValue)
        {
            var toTerm = await _context.Terms.FindAsync(createVersionDto.ToTermId.Value);
            if (toTerm == null)
            {
                throw new ArgumentException($"Term with ID {createVersionDto.ToTermId.Value} not found");
            }
        }

        int nextVersionNumber = course.CourseVersions.Any()
            ? course.CourseVersions.Max(cv => cv.VersionNumber) + 1
            : 1;

        CourseVersion courseVersion = new CourseVersion
        {
            CourseId = courseId,
            Description = createVersionDto.Description,
            AimAndObjectives = createVersionDto.AimAndObjectives,
            CourseContent = createVersionDto.CourseContent,
            CILOs = createVersionDto.CILOs,
            TLAs = createVersionDto.TLAs,
            VersionNumber = nextVersionNumber,
            FromYear = createVersionDto.FromYear,
            FromTermId = createVersionDto.FromTermId,
            ToYear = createVersionDto.ToYear,
            ToTermId = createVersionDto.ToTermId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CourseVersions.Add(courseVersion);

        if (createVersionDto.MediumOfInstructionIds.Count > 0)
        {
            List<int> mediumIds = createVersionDto.MediumOfInstructionIds;

            List<int> existingMediumIds = await _context.MediumOfInstructions
                .Where(m => mediumIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            List<int> missingMediums = mediumIds.Except(existingMediumIds).ToList();
            if (missingMediums.Any())
            {
                throw new ArgumentException(
                    $"Medium of instruction(s) with ID(s) {string.Join(',', missingMediums)} not found");
            }

            foreach (var mediumId in mediumIds)
            {
                var courseVersionMedium = new CourseVersionMedium
                {
                    CourseVersion = courseVersion,
                    MediumOfInstructionId = mediumId
                };
                _context.CourseVersionMediums.Add(courseVersionMedium);
            }
        }

        if (createVersionDto.Assessments.Count > 0)
        {
            foreach (var assessmentDto in createVersionDto.Assessments)
            {
                CourseAssessment assessment = new CourseAssessment
                {
                    CourseVersion = courseVersion,
                    Name = assessmentDto.Name,
                    Weighting = assessmentDto.Weighting,
                    Category = assessmentDto.Category,
                    Description = assessmentDto.Description
                };
                _context.CourseAssessments.Add(assessment);
            }
        }


        if (createVersionDto.PreRequisiteCourseVersionIds.Count > 0)
        {
            List<int> distinctPreReqIds = createVersionDto.PreRequisiteCourseVersionIds.Distinct().ToList();

            List<int> existingPreReqIds = await _context.CourseVersions
                .Where(cv => distinctPreReqIds.Contains(cv.Id))
                .Select(cv => cv.Id)
                .ToListAsync();

            List<int> missingPreReqs = distinctPreReqIds.Except(existingPreReqIds).ToList();
            if (missingPreReqs.Any())
            {
                throw new ArgumentException(
                    $"Course version(s) with ID(s) {string.Join(',', missingPreReqs)} not found for prerequisites");
            }

            foreach (int preReqId in distinctPreReqIds)
            {
                CoursePreReq coursePreReq = new CoursePreReq
                {
                    CourseVersion = courseVersion,
                    RequiredCourseVersionId = preReqId
                };
                _context.CoursePreReqs.Add(coursePreReq);
            }
        }

        if (createVersionDto.AntiRequisiteCourseVersionIds.Count > 0)
        {
            List<int> distinctAntiReqIds = createVersionDto.AntiRequisiteCourseVersionIds.Distinct().ToList();

            List<int> existingAntiReqIds = await _context.CourseVersions
                .Where(cv => distinctAntiReqIds.Contains(cv.Id))
                .Select(cv => cv.Id)
                .ToListAsync();

            List<int> missingAntiReqs = distinctAntiReqIds.Except(existingAntiReqIds).ToList();
            if (missingAntiReqs.Any())
            {
                throw new ArgumentException(
                    $"Course version(s) with ID(s) {string.Join(',', missingAntiReqs)} not found for anti-requisites");
            }

            foreach (int antiReqId in distinctAntiReqIds)
            {
                CourseAntiReq courseAntiReq = new CourseAntiReq
                {
                    CourseVersion = courseVersion,
                    ExcludedCourseVersionId = antiReqId
                };
                _context.CourseAntiReqs.Add(courseAntiReq);
            }
        }

        await _context.SaveChangesAsync();

        try
        {
            await GenerateCourseEmbeddings(course, createVersionDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Embedding generation failed for course version {courseVersion.Id}: {ex.Message}");
        }

        return courseVersion.Id;
    }

    //ya i dunno, so I can skip can only test this
    private async Task GenerateCourseEmbeddings(Course course, CreateCourseVersionDto createVersionDto)
    {
        //This tag things is hard to have high similarity, so I set a low threshold
        IAIProvider aiProvider = ((AIProviderFactory)_aiProviderFactory).GetDefaultProvider();

        var assessmentMethods = createVersionDto.Assessments.Select(a => new AssessmentMethod
        {
            Name = a.Name,
            Weighting = a.Weighting,
            Category = a.Category,
            Description = a.Description ?? string.Empty
        }).ToList();

        //I think I should just based on the course code, as a domain tag.

        // var domainEmbedding = await aiProvider.CreateCourseDomainTagEmbeddingAsync(
        //     courseTitle: courseName,
        //     aimsAndObjectives: createVersionDto.AimAndObjectives,
        //     courseContent: createVersionDto.CourseContent
        // );
        //
        // var domainTags = await GetSimilarCourseTags(domainEmbedding, TagType.Domain);


        var skillsEmbedding = await aiProvider.CreateCourseSkillsTagEmbeddingAsync(
            aimsAndObjectives: createVersionDto.AimAndObjectives,
            cilos: createVersionDto.CILOs,
            courseContent: createVersionDto.CourseContent,
            tlas: createVersionDto.TLAs,
            assessmentMethods: assessmentMethods
        );

        var skillsTags = await GetSimilarCourseTags(skillsEmbedding, TagType.Skill);
        foreach (var skillsTag in skillsTags)
        {
            CourseTag courseTag = new CourseTag()
            {
                Course = course,
                Tag = skillsTag
            };
            _context.CourseTags.Add(courseTag);
        }

        var contentTypesEmbedding = await aiProvider.CreateCourseContentTypesTagEmbeddingAsync(
            courseContent: createVersionDto.CourseContent,
            tlas: createVersionDto.TLAs,
            assessmentMethods: assessmentMethods
        );

        var contentTypesTags = await GetSimilarCourseTags(contentTypesEmbedding, TagType.ContentType);
        foreach (var contentTypeTag in contentTypesTags)
        {
            CourseTag courseTag = new CourseTag()
            {
                Course = course,
                Tag = contentTypeTag
            };
            _context.CourseTags.Add(courseTag);
        }
        await _context.SaveChangesAsync();
    }

    //and this could be in AIProvider, since different embedding models may have different embedding outputs
    public async Task<List<Tag>> GetSimilarCourseTags(
        Vector queryVector,
        TagType tagType,
        int topK = 5,
        double minDot = 0.25
    )
    {
        //I tired to use one Query but it don't work, I think it need raw SQL
        //so let me do it in memory
        var allTags = await _context.Tags
            .Where(t => t.TagType == tagType)
            .ToListAsync();

        var scored = allTags
            .Where(t => t.Embedding != null)
            .Select(t => new
            {
                Tag = t,
                Dot = DotProduct(t.Embedding, queryVector) // HIGHER is more similar
            })
            .OrderByDescending(x => x.Dot)
            .Take(topK)
            .ToList();

        return scored.Select(x => x.Tag).ToList();
    }

    public static double DotProduct(Vector a, Vector b)
    {
        //openai embedding is normalized to length 1, so dot product is cosine similarity
        var aSpan = a.Memory.Span;
        var bSpan = b.Memory.Span;

        if (aSpan.Length != bSpan.Length)
            throw new ArgumentException("Vectors must have the same length.");

        double sum = 0;
        for (int i = 0; i < aSpan.Length; i++)
            sum += (double)aSpan[i] * bSpan[i];

        return sum;
    }


    public async Task<TimetableResponseDto> GetTimetableAsync(string studentId, bool excludeEnrolled = true,
        int? year = null, int? termId = null, int? courseGroupId = null, int? categoryGroupId = null)
    {
        int defaultYear = year ?? _factService.GetCurrentAcademicYear();
        int defaultTermId = termId ?? _factService.GetCurrentSemester();

        Term? term = await _context.Terms.FindAsync(defaultTermId);
        if (term == null)
        {
            throw new ArgumentException($"Term with ID {defaultTermId} not found");
        }

        HashSet<int>? allowedCourseIds = null;

        if (courseGroupId.HasValue || categoryGroupId.HasValue)
        {
            allowedCourseIds = new HashSet<int>();

            if (courseGroupId.HasValue)
            {
                var coursesInGroup = await _context.GroupCourses
                    .Where(gc => gc.GroupId == courseGroupId.Value && gc.CourseId.HasValue)
                    .Select(gc => gc.CourseId!.Value)
                    .ToListAsync();

                foreach (var courseId in coursesInGroup)
                {
                    allowedCourseIds.Add(courseId);
                }

                var codesInGroup = await _context.GroupCourses
                    .Where(gc => gc.GroupId == courseGroupId.Value && gc.CodeId.HasValue)
                    .Select(gc => gc.CodeId!.Value)
                    .ToListAsync();

                if (codesInGroup.Any())
                {
                    var coursesByCode = await _context.Courses
                        .Where(c => codesInGroup.Contains(c.CodeId))
                        .Select(c => c.Id)
                        .ToListAsync();

                    foreach (var courseId in coursesByCode)
                    {
                        allowedCourseIds.Add(courseId);
                    }
                }
            }

            if (categoryGroupId.HasValue)
            {
                var groupsInCategory = await _context.CategoryGroups
                    .Where(cg => cg.CategoryId == categoryGroupId.Value && cg.GroupId.HasValue)
                    .Select(cg => cg.GroupId!.Value)
                    .ToListAsync();

                if (groupsInCategory.Any())
                {
                    var coursesInCategoryGroup = await _context.GroupCourses
                        .Where(gc => groupsInCategory.Contains(gc.GroupId) && gc.CourseId.HasValue)
                        .Select(gc => gc.CourseId!.Value)
                        .ToListAsync();

                    foreach (var courseId in coursesInCategoryGroup)
                    {
                        allowedCourseIds.Add(courseId);
                    }

                    var codesInCategoryGroup = await _context.GroupCourses
                        .Where(gc => groupsInCategory.Contains(gc.GroupId) && gc.CodeId.HasValue)
                        .Select(gc => gc.CodeId!.Value)
                        .ToListAsync();

                    if (codesInCategoryGroup.Any())
                    {
                        var coursesByCode = await _context.Courses
                            .Where(c => codesInCategoryGroup.Contains(c.CodeId))
                            .Select(c => c.Id)
                            .ToListAsync();

                        foreach (var courseId in coursesByCode)
                        {
                            allowedCourseIds.Add(courseId);
                        }
                    }
                }
            }
        }

        List<CourseSection> sections = new List<CourseSection>();

        if (excludeEnrolled)
        {
            var enrolledCourseIds = await _context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => sc.CourseId)
                .ToListAsync();

            var query = _context.CourseSections
                .Where(sc => sc.Year == defaultYear && sc.TermId == defaultTermId)
                .Where(sc => !enrolledCourseIds.Contains(sc.CourseVersion.CourseId));

            if (allowedCourseIds != null && allowedCourseIds.Any())
            {
                query = query.Where(sc => allowedCourseIds.Contains(sc.CourseVersion.CourseId));
            }

            sections = await query
                .Include(cs => cs.CourseVersion)
                .ThenInclude(cv => cv.Course)
                .ThenInclude(c => c.Code)
                .Include(cs => cs.CourseMeetings)
                .Include(cs => cs.Term)
                .ToListAsync();
        }
        else
        {
            var query = _context.CourseSections
                .Where(cs => cs.Year == defaultYear && cs.TermId == defaultTermId);

            if (allowedCourseIds != null && allowedCourseIds.Any())
            {
                query = query.Where(cs => allowedCourseIds.Contains(cs.CourseVersion.CourseId));
            }

            sections = await query
                .Include(cs => cs.CourseVersion)
                .ThenInclude(cv => cv.Course)
                .ThenInclude(c => c.Code)
                .Include(cs => cs.CourseMeetings)
                .Include(cs => cs.Term)
                .ToListAsync();
        }


        List<TimetableEntryDto> entries = sections
            .GroupBy(section => new
            {
                section.CourseVersionId,
                section.CourseVersion.VersionNumber,
                section.CourseVersion.CourseId,
                CourseName = section.CourseVersion.Course.Name,
                CourseCode = $"{section.CourseVersion.Course.Code.Tag}{section.CourseVersion.Course.CourseNumber}",
                CodeTag = section.CourseVersion.Course.Code.Tag,
                CourseNumber = section.CourseVersion.Course.CourseNumber,
                Credit = section.CourseVersion.Course.Credit
            })
            .Select(group => new TimetableEntryDto
            {
                CourseId = group.Key.CourseId,
                CourseName = group.Key.CourseName,
                CourseCode = group.Key.CourseCode,
                CodeTag = group.Key.CodeTag,
                CourseNumber = group.Key.CourseNumber,
                Credit = group.Key.Credit,
                VersionId = group.Key.CourseVersionId,
                VersionNumber = group.Key.VersionNumber,
                Sections = group.Select(section => new TimetableSectionDto
                {
                    SectionId = section.Id,
                    SectionNumber = section.SectionNumber,
                    Meetings = section.CourseMeetings.Select(m => new TimetableMeetingDto
                    {
                        Id = m.Id,
                        MeetingType = m.MeetingType,
                        Day = m.Day,
                        StartTime = m.StartTime,
                        EndTime = m.EndTime
                    }).ToList()
                }).ToList()
            })
            .ToList();

        return new TimetableResponseDto
        {
            Year = defaultYear,
            Term = term.Name,
            Entries = entries
        };
    }
}