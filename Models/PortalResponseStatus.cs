using System.Text.Json.Serialization;

namespace ClassRegisterApp.Models;

/// <summary>
///     Present Status response from server
/// </summary>
public class PortalResponseStatus
{
    /// <summary>
    /// </summary>
    /// <param name="msg"></param>
    [JsonConstructor]
    public PortalResponseStatus(string? msg)
    {
        Msg = msg;
    }

    /// <summary>
    ///     Message
    /// </summary>
    public string? Msg { get; set; }

    /// <summary>
    ///     Http Status
    /// </summary>
    public bool? Status { get; set; }
}
