using HeartCheck.Models;
using MongoDB.Bson;

namespace HeartCheck.Data
{
    public interface IAuditLogRepository
    {
        Task CreateAsync(AuditLog log);
        Task<AuditLog?> GetByIdAsync(ObjectId id);
        Task<List<AuditLog>> GetFilteredAsync(
            ObjectId? userId, string? action, string? entity,
            ObjectId? entityId, DateTime? from, DateTime? to);
    }
}
