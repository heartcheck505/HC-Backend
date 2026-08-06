using System.Security.Claims;
using HeartCheck.DTOs.EmergencyCalls;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/emergency-calls")]
    [Authorize]
    public class EmergencyCallController : ControllerBase
    {
        private readonly IEmergencyCallService _emergencyCallService;

        public EmergencyCallController(IEmergencyCallService emergencyCallService)
        {
            _emergencyCallService = emergencyCallService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmergencyCallRequest request)
        {
            try
            {
                var userId = GetUserId();
                var call = await _emergencyCallService.CreateEmergencyCallAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { id = call.Id }, call);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCalls()
        {
            try
            {
                var userId = GetUserId();
                var calls = await _emergencyCallService.GetUserCallsAsync(userId);
                return Ok(calls);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var userId = GetUserId();
                var callId = ObjectId.Parse(id);
                var call = await _emergencyCallService.GetCallByIdAsync(userId, callId);
                return Ok(call);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateEmergencyCallStatusRequest request)
        {
            try
            {
                var userId = GetUserId();
                var callId = ObjectId.Parse(id);
                await _emergencyCallService.UpdateCallStatusAsync(userId, callId, request);
                return NoContent();
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

        private ObjectId GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return ObjectId.Parse(userIdClaim);
        }
    }
}
