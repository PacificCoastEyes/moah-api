using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace moah_api.Models;

public class JournalEntry
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement("user_id")]
    public required ObjectId UserId { get; set; }
    [BsonElement("content")]
    public string? Content { get; set; }
}