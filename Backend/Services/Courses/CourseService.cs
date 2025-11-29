using System.Text;
using System.Text.RegularExpressions;
using Backend.Models;
using Backend.Services.AI;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Backend.Services.Courses;

public class ParsedCourseData
{
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string NoOfUnits { get; set; } = string.Empty;
    public string OfferingDepartment { get; set; } = string.Empty;
    public string Prerequisites { get; set; } = string.Empty;
    public string MediumOfInstruction { get; set; } = string.Empty;
    public string AimsObjectives { get; set; } = string.Empty;
    public string CourseContent { get; set; } = string.Empty;
    public string CILOsRaw { get; set; } = string.Empty;
    public string TLAsRaw { get; set; } = string.Empty;
    public string AssessmentMethodsRaw { get; set; } = string.Empty;
    public List<CILOs> CILOs { get; set; } = new();
    public List<TLAs> TLAs { get; set; } = new();
    public List<AssessmentMethod> AssessmentMethods { get; set; } = new();
}

public class PdfProcessingResult
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Pages { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public ParsedCourseData ParsedData { get; set; } = new();
}

public interface ICourseService
{
    Task<PdfProcessingResult> ProcessPdfAsync(Stream pdfStream, string fileName, long fileSize, AIProviderType? providerType = null);
}


public class CourseService : ICourseService
{
    private readonly ILogger<CourseService> _logger;
    private readonly IAIProviderFactory _aiProviderFactory;

    public CourseService(ILogger<CourseService> logger, IAIProviderFactory aiProviderFactory)
    {
        _logger = logger;
        _aiProviderFactory = aiProviderFactory;
    }

    private ParsedCourseData ParseCourseContent(string fullText)
    {
        Dictionary<string, string> sections = new Dictionary<string, string>();

        Dictionary<string, string> sectionPatterns = new Dictionary<string, string>
        {
            { "COURSE_TITLE", @"1\.\s*COURSE TITLE[:\s]*(.+?)(?=2\.\s*COURSE CODE|$)" },
            { "COURSE_CODE", @"2\.\s*COURSE CODE[:\s]*(.+?)(?=3\.\s*NO\.\s*OF UNITS|$)" },
            { "NO_OF_UNITS", @"3\.\s*NO\.\s*OF UNITS[:\s]*(.+?)(?=4\.\s*OFFERING DEPARTMENT|$)" },
            { "OFFERING_DEPARTMENT", @"4\.\s*OFFERING DEPARTMENT[:\s]*(.+?)(?=5\.\s*PREREQUISITES|$)" },
            { "PREREQUISITES", @"5\.\s*PREREQUISITES[:\s]*(.+?)(?=6\.\s*MEDIUM OF INSTRUCTION|$)" },
            { "MEDIUM_OF_INSTRUCTION", @"6\.\s*MEDIUM OF INSTRUCTION[:\s]*(.+?)(?=7\.\s*AIMS\s*[&AND]*\s*OBJECTIVES|$)" },
            { "AIMS_OBJECTIVES", @"7\.\s*AIMS\s*[&AND]*\s*OBJECTIVES[:\s]*(.+?)(?=8\.\s*COURSE CONTENT|$)" },
            { "COURSE_CONTENT", @"8\.\s*COURSE CONTENT[:\s]*(.+?)(?=9\.\s*COURSE INTENDED LEARNING OUTCOMES|$)" },
            { "CILOS", @"9\.\s*COURSE INTENDED LEARNING OUTCOMES\s*\(?CILOs?\)?[:\s]*(.+?)(?=10\.\s*TEACHING\s*[&AND]*\s*LEARNING ACTIVITIES|$)" },
            { "TLAS", @"10\.\s*TEACHING\s*[&AND]*\s*LEARNING ACTIVITIES\s*\(?TLAs?\)?[:\s]*(.+?)(?=11\.\s*ASSESSMENT METHODS|$)" },
            { "ASSESSMENT_METHODS", @"11\.\s*ASSESSMENT METHODS\s*\(?AMs?\)?[:\s]*(.+?)$" }
        };

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

        return new ParsedCourseData
        {
            CourseTitle = sections["COURSE_TITLE"],
            CourseCode = sections["COURSE_CODE"],
            NoOfUnits = sections["NO_OF_UNITS"],
            OfferingDepartment = sections["OFFERING_DEPARTMENT"],
            Prerequisites = sections["PREREQUISITES"],
            MediumOfInstruction = sections["MEDIUM_OF_INSTRUCTION"],
            AimsObjectives = sections["AIMS_OBJECTIVES"],
            CourseContent = sections["COURSE_CONTENT"],
            CILOsRaw = sections["CILOS"],
            TLAsRaw = sections["TLAS"],
            AssessmentMethodsRaw = sections["ASSESSMENT_METHODS"]
        };
    }

    // this bro is to extract text from pdf and parse it into sections
    public async Task<PdfProcessingResult> ProcessPdfAsync(Stream pdfStream, string fileName, long fileSize, AIProviderType? providerType = null)
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
        ParsedCourseData parsedSections = ParseCourseContent(fullText);

        await ExtractDataWithAI(parsedSections, providerType);

        return new PdfProcessingResult
        {
            FileName = fileName,
            FileSize = fileSize,
            Pages = document.NumberOfPages,
            ExtractedText = fullText,
            ParsedData = parsedSections
        };
    }

    public async Task ExtractDataWithAI(ParsedCourseData parsedCourseData, AIProviderType? providerType = null)
    {
        IAIProvider provider = providerType.HasValue
                ? _aiProviderFactory.GetProvider(providerType.Value)
                : ((AIProviderFactory)_aiProviderFactory).GetDefaultProvider();

        Task<List<CILOs>>? cilosTask = !string.IsNullOrWhiteSpace(parsedCourseData.CILOsRaw)
            ? provider.ExtractCILOsAsync(parsedCourseData.CILOsRaw)
            : Task.FromResult(new List<CILOs>());

        Task<List<AssessmentMethod>>? assessmentTask = !string.IsNullOrWhiteSpace(parsedCourseData.AssessmentMethodsRaw)
            ? provider.ExtractAssessmentMethodsAsync(parsedCourseData.AssessmentMethodsRaw)
            : Task.FromResult(new List<AssessmentMethod>());

        Task<List<TLAs>>? tlasTask = !string.IsNullOrWhiteSpace(parsedCourseData.TLAsRaw)
            ? provider.ExtractTLAsAsync(parsedCourseData.TLAsRaw)
            : Task.FromResult(new List<TLAs>());

        await Task.WhenAll(cilosTask, assessmentTask, tlasTask);

        parsedCourseData.CILOs = await cilosTask;
        parsedCourseData.AssessmentMethods = await assessmentTask;
        parsedCourseData.TLAs = await tlasTask;
    }
}
