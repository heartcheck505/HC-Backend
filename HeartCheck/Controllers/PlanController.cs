using HeartCheck.DTOs.Plans;
using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/plans")]
    [Authorize]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivePlans()
        {
            var plans = await _planService.GetActivePlansAsync();
            return Ok(plans);
        }
    }
}
