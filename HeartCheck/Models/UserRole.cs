using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    public class UserRole
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("roleId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId RoleId { get; set; }

        [BsonElement("assignedAt")]
        public DateTime AssignedAt { get; set; }

        [BsonElement("assignedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? AssignedBy { get; set; }
    }
}
