﻿﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Services.User;
using Backend.Dtos.Courses;
using Backend.Dtos.Programmes;
using System.Security.Claims;
using Backend.Dtos.User;
using Backend.Services.Programmes;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly ILogger<MeController> _logger;
        private readonly IUserService _userService;
        private readonly IEvaluateRule _evaluateRule;
        private readonly IProgrammeService _programmeService;

        public MeController(ILogger<MeController> logger, IUserService userService, IEvaluateRule evaluateRule, IProgrammeService programmeService)
        {
            _logger = logger;
            _userService = userService;
            _evaluateRule = evaluateRule;
            _programmeService = programmeService;
        }

        [HttpGet("courses")]
        [Authorize]
        [ProducesResponseType(typeof(List<UserCourseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserCourses()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var courses = await _userService.GetStudentStudyCoursesAsync(userId);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user courses");
                return StatusCode(500, new { message = "Error retrieving user courses" });
            }
        }

        [HttpPost("courses")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCourses([FromBody] List<CreateStudentCourseDto> courses)
        {
            try
            {
                if (courses.Count == 0)
                {
                    return BadRequest(new { message = "Courses array cannot be empty" });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var success = await _userService.AddStudentStudyCoursesAsync(userId, courses);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to add courses" });
                }

                return CreatedAtAction(nameof(GetUserCourses), new { }, new { message = "Courses added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user courses");
                return StatusCode(500, new { message = "Error adding user courses" });
            }
        }

        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(typeof(IList<CategoryCompletionStatus>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckUserCompletion()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var result = await _evaluateRule.CheckUserCompletion(userId);
            return Ok(result);
        }

        [HttpGet("academic-progress")]
        [Authorize]
        [ProducesResponseType(typeof(AcademicProgressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAcademicProgress()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var progress = await _userService.GetAcademicProgressAsync(userId);
                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving academic progress");
                return StatusCode(500, new { message = "Error retrieving academic progress" });
            }
        }

        [HttpGet("programme")]
        [Authorize]
        [ProducesResponseType(typeof(UserProgrammeDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserProgrammeDetail()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var programmeDetail = await _programmeService.GetUserProgrammeDetailAsync(userId);
                if (programmeDetail == null)
                {
                    return NotFound(new { message = "No programme found for the user" });
                }

                return Ok(programmeDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user programme detail");
                return StatusCode(500, new { message = "Error retrieving user programme detail" });
            }
        }

        [HttpGet("category-groups")]
        [Authorize]
        [ProducesResponseType(typeof(List<CategoryGroupResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryGroups()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var categoryGroups = await _programmeService.GetCategoryGroupsAsync(userId);
                return Ok(categoryGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category groups");
                return StatusCode(500, new { message = "Error retrieving category groups" });
            }
        }

        [HttpGet("suggested-schedule")]
        [Authorize]
        [ProducesResponseType(typeof(SuggestedScheduleResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSuggestedSchedule()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var schedule = await _userService.GetSuggestedScheduleAsync(userId);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suggested schedule");
                return StatusCode(500, new { message = "Error retrieving suggested schedule" });
            }
        }
    }
}
