using System;
using System.Text.Json.Serialization;

namespace ClassRegisterApp.Models;

public class Code
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("code")]
    public string CodeString { get; set; } = null!;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = new();

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = new();

    [JsonPropertyName("expiredAt")]
    public DateTime ExpiredAt { get; set; } = new();

    [JsonPropertyName("codeTypeId")]
    public string CodeTypeId { get; set; } = null!;
}
