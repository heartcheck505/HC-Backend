using System.Security.Claims;
using HeartCheck.DTOs.Patients;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _patientService.CreateAsync(userId, request);
                return CreatedAtAction(nameof(GetMe), null, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = GetUserId();
                var response = await _patientService.GetByUserIdAsync(userId);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdatePatientRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _patientService.UpdateAsync(userId, request);
                return Ok(response);
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
