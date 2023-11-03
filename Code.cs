using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassRegisterApp;

internal class Code
{
    public int DayExpired { get; set; } = 1;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.String)] public string CodeString { get; set; } = null!;

    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Time { get; set; } = DateTime.Now;
}