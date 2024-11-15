using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseTokenResponse
{
    [JsonPropertyName("sessionID")]
    public string? SessionId { get; set; }
    [JsonPropertyName("targetTimeZone")]
    public string? TargetTimeZone { get; set; }
    [JsonPropertyName("timeZone")]
    public string? TimeZone { get; set; }
    [JsonPropertyName("token")]
    public string? Token { get; set; }
    
}