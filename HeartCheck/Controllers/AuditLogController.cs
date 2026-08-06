using HeartCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace HeartCheck.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string? userId,
            [FromQuery] string? action,
            [FromQuery] string? entity,
            [FromQuery] string? entityId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            try
            {
                var logs = await _auditLogService.GetAuditLogsAsync(
                    ParseOptionalObjectId(userId),
                    action,
                    entity,
                    ParseOptionalObjectId(entityId),
                    from,
                    to);

                return Ok(logs);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLogById(string id)
        {
            try
            {
                var log = await _auditLogService.GetAuditLogByIdAsync(ObjectId.Parse(id));
                return Ok(log);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private static ObjectId? ParseOptionalObjectId(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : ObjectId.Parse(value);
        }
    }
}
