using System.Security.Claims;
using HeartCheck.DTOs.Plans;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/user-plans")]
    [Authorize]
    public class UserPlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public UserPlanController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpPost]
        public async Task<IActionResult> AssignPlan([FromBody] AssignUserPlanRequest request)
        {
            try
            {
                var userId = GetUserId();
                var response = await _planService.AssignPlanToUserAsync(userId, request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyActivePlan()
        {
            try
            {
                var userId = GetUserId();
                var response = await _planService.GetUserActivePlanAsync(userId);
                if (response == null)
                {
                    return NotFound(new { message = "No active plan found" });
                }
                return Ok(response);
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
