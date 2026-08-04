using System.Security.Claims;
using HeartCheck.DTOs.Measurements;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/measurements")]
    [Authorize]
    public class MeasurementController : ControllerBase
    {
        private readonly IMeasurementService _measurementService;

        public MeasurementController(IMeasurementService measurementService)
        {
            _measurementService = measurementService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMeasurementRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _measurementService.CreateAsync(userId, request);
                return CreatedAtAction(nameof(GetHistory), null, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var userId = GetUserId();
                var measurements = await _measurementService.GetHistoryAsync(userId, from, to);
                return Ok(measurements);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private ObjectId GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ObjectId.Parse(userIdClaim);
        }
    }
}
