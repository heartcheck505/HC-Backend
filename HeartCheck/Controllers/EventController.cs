using System.Security.Claims;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("alert/{alertId}")]
        public async Task<IActionResult> GetByAlertId(string alertId)
        {
            try
            {
                var userId = GetUserId();
                var alertObjectId = ObjectId.Parse(alertId);
                var events = await _eventService.GetByAlertIdAsync(userId, alertObjectId);
                return Ok(events);
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
