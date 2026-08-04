using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HeartCheck.Models
{
    public class Role
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("permissions")]
        public List<string> Permissions { get; set; } = new();

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
