using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseAttribute
{
    [JsonPropertyName("name")]
    public string? Name { get; }

    [JsonPropertyName("displayValue")]
    public string? DisplayValue { get; }
}