using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace YTM.Core.Entities
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = null!;

        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("Price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("Brand")]
        public string? Brand { get; set; }

        [BsonElement("ImageUrl")]
        public string? ImageUrl { get; set; }

        [BsonElement("Stock")]
        public int Stock { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;
    }
} 