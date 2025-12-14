using Microsoft.AspNetCore.Mvc;
using Backend.Services.Courses;
using Backend.Services.AI;
using Backend.Dtos.Courses;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/courses")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ILogger<CoursesController> _logger;
        private readonly ICourseService _courseService;

        public CoursesController(ILogger<CoursesController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CourseResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourses([FromQuery] bool excludeEnrolled = false)
        {
            try
            {
                List<CourseResponseDto> courses;
                if (excludeEnrolled)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized(new { message = "User ID not found in token" });
                    }
                    courses = await _courseService.GetCoursesAsync(userId);
                }
                else
                {
                    courses = await _courseService.GetCoursesAsync();
                }
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                return StatusCode(500, new { message = "Error retrieving courses" });
            }
        }

        [HttpGet("simple")]
        [ProducesResponseType(typeof(List<SimpleCourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSimpleCourses()
        {
            try
            {
                var courses = await _courseService.GetSimpleCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving simple courses");
                return StatusCode(500, new { message = "Error retrieving simple courses" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                {
                    return NotFound(new { message = $"Course with ID {id} not found" });
                }
                return Ok(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course with ID {CourseId}", id);
                return StatusCode(500, new { message = "Error retrieving course" });
            }
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

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
        {
            try
            {
                var courseId = await _courseService.CreateCourseAsync(createCourseDto);
                return Created($"/api/courses/{courseId}", new { id = courseId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating course");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating course");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { message = "Error creating course" });
            }
        }

        [HttpPost("{id}/versions")]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCourseVersion(int id, [FromBody] CreateCourseVersionDto createVersionDto)
        {
            try
            {
                var versionId = await _courseService.CreateCourseVersionAsync(id, createVersionDto);
                return Created($"/api/courses/{id}/versions/{versionId}", new { id = versionId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating course version");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course version");
                return StatusCode(500, new { message = "Error creating course version" });
            }
        }
    }
}
