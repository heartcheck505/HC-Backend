using HeartCheck.DTOs.AuditLogs;
using MongoDB.Bson;

namespace HeartCheck.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(
            ObjectId userId, string action, string entity, ObjectId entityId,
            string ipAddress, string? userAgent,
            object? previousValues = null, object? newValues = null);
        Task<List<AuditLogResponse>> GetAuditLogsAsync(
            ObjectId? userId, string? action, string? entity,
            ObjectId? entityId, DateTime? from, DateTime? to);
        Task<AuditLogResponse> GetAuditLogByIdAsync(ObjectId id);
    }
}
