using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassRegisterApp.Models;

public class Code
{
    [JsonPropertyName("dayExpired")]
    public int DayExpired { get; set; }

    [JsonPropertyName("id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [JsonPropertyName("codeString")]
    [BsonRepresentation(BsonType.String)] public string CodeString { get; set; } = null!;

    [JsonPropertyName("time")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Time { get; set; } = DateTime.Now;

    [JsonPropertyName("delay")]
    [BsonRepresentation(BsonType.Int32)] public int Delay { get; set; }
}
