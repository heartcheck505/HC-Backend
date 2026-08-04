using System.Security.Claims;
using HeartCheck.DTOs.Alerts;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    [Authorize]
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _alertService;

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                var userId = GetUserId();
                var alerts = await _alertService.GetActiveAlertsByPatientAsync(userId);
                return Ok(alerts);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/acknowledge")]
        public async Task<IActionResult> Acknowledge(string id, [FromBody] AcknowledgeAlertRequest request)
        {
            try
            {
                var userId = GetUserId();
                var alertId = ObjectId.Parse(id);
                await _alertService.AcknowledgeAlertAsync(userId, alertId, request?.UserResponse);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> Resolve(string id)
        {
            try
            {
                var userId = GetUserId();
                var alertId = ObjectId.Parse(id);
                await _alertService.ResolveAlertAsync(userId, alertId);
                return NoContent();
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
