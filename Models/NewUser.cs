using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace moah_api.Models;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }
    [BsonElement("first_name")]
    public required string FirstName { get; set; }
    [BsonElement("email")]
    public required string Email { get; set; }
    [BsonElement("password")]
    public required string Password { get; set; }

}