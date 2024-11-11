using System;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassRegisterApp.Models;

public class Code
{
    /// <summary>
    /// Số ngày hết hạn của code tính từ ngày tạo
    /// </summary>
    [JsonPropertyName("dayExpired")]
    public int DayExpired { get; set; }

    /// <summary>
    /// Id của code
    /// </summary>
    [JsonPropertyName("id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Mã code
    /// </summary>
    [JsonPropertyName("codeString")]
    [BsonRepresentation(BsonType.String)] public string CodeString { get; set; } = null!;

    /// <summary>
    /// Thời gian tạo code
    /// </summary>
    [JsonPropertyName("time")]
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime Time { get; set; } = DateTime.Now;

    /// <summary>
    /// Delay giữa các lần sử dụng code
    /// </summary>
    [JsonPropertyName("delay")]
    [BsonRepresentation(BsonType.Int32)] public int Delay { get; set; }
}
