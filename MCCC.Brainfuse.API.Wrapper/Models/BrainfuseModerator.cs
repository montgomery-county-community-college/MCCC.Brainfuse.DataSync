using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseModerator
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("status")]
    public long Status { get; set; }

    [JsonPropertyName("uid")]
    public long Uid { get; set; }

    [JsonPropertyName("finalFullDisplayName")]
    public string? FinalFullDisplayName { get; set; }

    [JsonPropertyName("moderator")]
    public long ModeratorModerator { get; set; }

    [JsonPropertyName("attendance")]
    public string? Attendance { get; set; }
}