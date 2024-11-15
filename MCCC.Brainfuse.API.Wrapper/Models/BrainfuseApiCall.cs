using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseApiCall<T>
{
    [JsonPropertyName("pages")]
    public BrainfusePages? BrainfusePages { get; init; }

    public T[]? Data { get; init; }

}