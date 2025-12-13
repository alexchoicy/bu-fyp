using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Backend.Services.Courses;
using Backend.Dtos.Courses;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/timetable")]
    public class TimetableController : ControllerBase
    {
        private readonly ILogger<TimetableController> _logger;
        private readonly ICourseService _courseService;

        public TimetableController(ILogger<TimetableController> logger, ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(TimetableResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTimetable([FromQuery] int? year = null, [FromQuery] int? termId = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }
                var timetable = await _courseService.GetTimetableAsync(userId, true ,year, termId);
                return Ok(timetable);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when retrieving timetable");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving timetable for year {Year} and term {TermId}", year, termId);
                return StatusCode(500, new { message = "Error retrieving timetable" });
            }
        }
    }
}
