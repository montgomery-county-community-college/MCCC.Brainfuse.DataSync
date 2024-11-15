using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfusePages
{
    [JsonPropertyName("numOfPages")]
    public long NumOfPages { get; set; }

    [JsonPropertyName("__sessionId")]
    public string? SessionId { get; set; }

    [JsonPropertyName("page")]
    public long Page { get; set; }
}