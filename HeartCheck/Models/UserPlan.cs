using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class UserPlan
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("planId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PlanId { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "active";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
