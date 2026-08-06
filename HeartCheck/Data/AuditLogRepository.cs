using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HeartCheck.Data
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly MongoDbContext _context;

        public AuditLogRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(AuditLog log)
        {
            await _context.AuditLogs.InsertOneAsync(log);
        }

        public async Task<AuditLog?> GetByIdAsync(ObjectId id)
        {
            return await _context.AuditLogs
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AuditLog>> GetFilteredAsync(
            ObjectId? userId, string? action, string? entity,
            ObjectId? entityId, DateTime? from, DateTime? to)
        {
            var filter = Builders<AuditLog>.Filter.Empty;

            if (userId.HasValue)
            {
                filter &= Builders<AuditLog>.Filter.Eq(l => l.UserId, userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                filter &= Builders<AuditLog>.Filter.Eq(l => l.Action, action);
            }

            if (!string.IsNullOrWhiteSpace(entity))
            {
                filter &= Builders<AuditLog>.Filter.Eq(l => l.Entity, entity);
            }

            if (entityId.HasValue)
            {
                filter &= Builders<AuditLog>.Filter.Eq(l => l.EntityId, entityId.Value);
            }

            if (from.HasValue)
            {
                filter &= Builders<AuditLog>.Filter.Gte(l => l.CreatedAt, from.Value);
            }

            if (to.HasValue)
            {
                filter &= Builders<AuditLog>.Filter.Lte(l => l.CreatedAt, to.Value);
            }

            return await _context.AuditLogs
                .Find(filter)
                .SortByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
