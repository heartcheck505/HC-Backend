using System.Text.Json;
using HeartCheck.Data;
using HeartCheck.DTOs.AuditLogs;
using HeartCheck.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace HeartCheck.Services
{
    public class AuditLogService : IAuditLogService
    {
        private static readonly HashSet<string> ValidActions = new()
        {
            "CREATE", "UPDATE", "DELETE", "LOGIN", "LOGOUT", "EXPORT"
        };

        private static readonly HashSet<string> ValidEntities = new()
        {
            "user", "patient", "device", "measurement",
            "alert", "plan", "notification", "setting"
        };

        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogAsync(
            ObjectId userId, string action, string entity, ObjectId entityId,
            string ipAddress, string? userAgent,
            object? previousValues = null, object? newValues = null)
        {
            if (!ValidActions.Contains(action))
            {
                throw new InvalidOperationException($"Invalid audit action: {action}");
            }

            if (!ValidEntities.Contains(entity))
            {
                throw new InvalidOperationException($"Invalid audit entity: {entity}");
            }

            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                PreviousValues = ToBsonDocument(previousValues),
                NewValues = ToBsonDocument(newValues),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.CreateAsync(log);
        }

        public async Task<List<AuditLogResponse>> GetAuditLogsAsync(
            ObjectId? userId, string? action, string? entity,
            ObjectId? entityId, DateTime? from, DateTime? to)
        {
            if (!string.IsNullOrWhiteSpace(action) && !ValidActions.Contains(action))
            {
                throw new InvalidOperationException($"Invalid audit action: {action}");
            }

            if (!string.IsNullOrWhiteSpace(entity) && !ValidEntities.Contains(entity))
            {
                throw new InvalidOperationException($"Invalid audit entity: {entity}");
            }

            var logs = await _auditLogRepository.GetFilteredAsync(
                userId, action, entity, entityId, from, to);

            return logs.Select(MapToResponse).ToList();
        }

        public async Task<AuditLogResponse> GetAuditLogByIdAsync(ObjectId id)
        {
            var log = await _auditLogRepository.GetByIdAsync(id);
            if (log == null)
            {
                throw new KeyNotFoundException("Audit log not found");
            }

            return MapToResponse(log);
        }

        private static BsonDocument? ToBsonDocument(object? values)
        {
            if (values == null)
            {
                return null;
            }

            return BsonSerializer.Deserialize<BsonDocument>(
                JsonSerializer.Serialize(values));
        }

        private static object? FromBsonValue(BsonValue value)
        {
            if (value == null || value.IsBsonNull)
            {
                return null;
            }

            return value.BsonType switch
            {
                BsonType.Int32 => value.AsInt32,
                BsonType.Int64 => value.AsInt64,
                BsonType.Double => value.AsDouble,
                BsonType.Decimal128 => Decimal128.ToDecimal(value.AsDecimal128),
                BsonType.String => value.AsString,
                BsonType.Boolean => value.AsBoolean,
                BsonType.ObjectId => value.AsObjectId.ToString(),
                BsonType.DateTime => value.ToUniversalTime(),
                BsonType.Array => value.AsBsonArray.Select(FromBsonValue).ToList(),
                BsonType.Document => value.AsBsonDocument.ToDictionary(
                    element => element.Name,
                    element => FromBsonValue(element.Value)),
                _ => value.ToString()
            };
        }

        private static AuditLogResponse MapToResponse(AuditLog log)
        {
            return new AuditLogResponse
            {
                Id = log.Id.ToString(),
                UserId = log.UserId.ToString(),
                Action = log.Action,
                Entity = log.Entity,
                EntityId = log.EntityId.ToString(),
                PreviousValues = log.PreviousValues == null
                    ? null
                    : log.PreviousValues.ToDictionary(
                        element => element.Name,
                        element => FromBsonValue(element.Value)),
                NewValues = log.NewValues == null
                    ? null
                    : log.NewValues.ToDictionary(
                        element => element.Name,
                        element => FromBsonValue(element.Value)),
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                CreatedAt = log.CreatedAt
            };
        }
    }
}
