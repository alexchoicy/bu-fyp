using Microsoft.AspNetCore.Mvc;
using Backend.Services.Facts;
using Backend.Dtos.Facts;

namespace Backend.Controllers;

[ApiController]
[Route("api/facts")]
public class FactsController : ControllerBase
{
    private readonly ILogger<FactsController> _logger;
    private readonly IFactService _factService;

    public FactsController(ILogger<FactsController> logger, IFactService factService)
    {
        _logger = logger;
        _factService = factService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CurrentFactsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentFacts()
    {
        try
        {
            var facts = await _factService.GetCurrentFactsAsync();
            return Ok(facts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current facts");
            return StatusCode(500, new { message = "Error retrieving facts" });
        }
    }

    [HttpGet("codes")]
    [ProducesResponseType(typeof(List<CodeResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCodes()
    {
        try
        {
            var codes = await _factService.GetCodesAsync();
            return Ok(codes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting codes");
            return StatusCode(500, new { message = "Error retrieving codes" });
        }
    }

    [HttpGet("groups")]
    [ProducesResponseType(typeof(List<CourseGroupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourseGroups()
    {
        try
        {
            var groups = await _factService.GetCourseGroupsAsync();
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course groups");
            return StatusCode(500, new { message = "Error retrieving course groups" });
        }
    }

    [HttpGet("departments")]
    [ProducesResponseType(typeof(List<DepartmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDepartments()
    {
        try
        {
            var departments = await _factService.GetDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            return StatusCode(500, new { message = "Error retrieving departments" });
        }
    }
}
