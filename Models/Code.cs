using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassRegisterApp.Models;

internal class Code
{
    public Code()
    {

    }


    public int DayExpired { get; set; }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.String)] public string CodeString { get; set; } = null!;

    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Time { get; set; } = DateTime.Now;

    [BsonRepresentation(BsonType.Int32)] public int Delay { get; set; }
}