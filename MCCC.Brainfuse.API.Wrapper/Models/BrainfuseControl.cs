using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseControl
{
    [JsonPropertyName("editable")]
    public bool Editable { get; set; }

    [JsonPropertyName("cancellable")]
    public bool Cancellable { get; set; }
}