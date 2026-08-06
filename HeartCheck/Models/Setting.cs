using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    [BsonIgnoreExtraElements]
    public class Setting
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("key")]
        public string Key { get; set; } = string.Empty;

        [BsonElement("value")]
        public BsonValue Value { get; set; } = BsonNull.Value;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("updatedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId? UpdatedBy { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
