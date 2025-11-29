using Microsoft.AspNetCore.Mvc;
using Backend.Services.Courses;
using Backend.Services.AI;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly ICourseService _courseService;

        public CoursesController(ILogger<CoursesController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [HttpPost("create/uploadPDF")]
        public async Task<IActionResult> UploadPDF(IFormFile file, [FromQuery] string? aiProvider = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (file.ContentType != "application/pdf" && !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "File must be a PDF" });
            }

            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds maximum limit of 10MB" });
            }

            try
            {
                using Stream stream = file.OpenReadStream();
                PdfProcessingResult result = await _courseService.ProcessPdfAsync(stream, file.FileName, file.Length);

                return Ok(new
                {
                    message = "PDF uploaded and processed successfully",
                    filename = result.FileName,
                    size = result.FileSize,
                    pages = result.Pages,
                    extractedText = result.ExtractedText,
                    parsedSections = new
                    {
                        courseTitle = result.ParsedData.CourseTitle,
                        courseCode = result.ParsedData.CourseCode,
                        noOfUnits = result.ParsedData.NoOfUnits,
                        offeringDepartment = result.ParsedData.OfferingDepartment,
                        prerequisites = result.ParsedData.Prerequisites,
                        mediumOfInstruction = result.ParsedData.MediumOfInstruction,
                        aimsObjectives = result.ParsedData.AimsObjectives,
                        courseContent = result.ParsedData.CourseContent,
                        cilosRaw = result.ParsedData.CILOsRaw,
                        cilos = result.ParsedData.CILOs,
                        tlasRaw = result.ParsedData.TLAsRaw,
                        tlas = result.ParsedData.TLAs,
                        assessmentMethodsRaw = result.ParsedData.AssessmentMethodsRaw,
                        assessmentMethods = result.ParsedData.AssessmentMethods
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF upload");
                return StatusCode(500, new { message = "Error processing PDF file" });
            }
        }
    }
}
