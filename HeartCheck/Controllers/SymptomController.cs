using System.Security.Claims;
using HeartCheck.DTOs.Symptoms;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/symptoms")]
    [Authorize]
    public class SymptomController : ControllerBase
    {
        private readonly ISymptomService _symptomService;

        public SymptomController(ISymptomService symptomService)
        {
            _symptomService = symptomService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSymptomRequest request)
        {
            try
            {
                var userId = GetUserId();
                var symptom = await _symptomService.CreateSymptomAsync(userId, request);
                return CreatedAtAction(nameof(GetById), new { id = symptom.Id }, symptom);
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
        public async Task<IActionResult> GetUserSymptoms()
        {
            try
            {
                var userId = GetUserId();
                var symptoms = await _symptomService.GetUserSymptomsAsync(userId);
                return Ok(symptoms);
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
                var symptomId = ObjectId.Parse(id);
                var symptom = await _symptomService.GetSymptomByIdAsync(userId, symptomId);
                return Ok(symptom);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("measurement/{measurementId}")]
        public async Task<IActionResult> GetByMeasurementId(string measurementId)
        {
            try
            {
                var userId = GetUserId();
                var measurementObjectId = ObjectId.Parse(measurementId);
                var symptoms = await _symptomService.GetByMeasurementIdAsync(userId, measurementObjectId);
                return Ok(symptoms);
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
