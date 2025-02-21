using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Search.WebApi.Domain.Products.Entities;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.Int32)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public decimal Price { get; set; }
    public decimal Step { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}