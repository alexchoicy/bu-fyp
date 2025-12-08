using Microsoft.AspNetCore.Mvc;
using Backend.Services.Courses;
using Backend.Services.AI;
using Backend.Dtos.Courses;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly ICourseService _courseService;

        public CoursesController(ILogger<CoursesController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [HttpPost("create/parsePDF")]
        [ProducesResponseType(typeof(PdfParseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ParsePDF(IFormFile file, [FromQuery] string? aiProvider = null)
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
                PdfParseResponseDto response = await _courseService.ProcessPdfAsync(stream, file.FileName, file.Length);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF parse");
                return StatusCode(500, new { message = "Error processing PDF file" });
            }
        }
    }
}
