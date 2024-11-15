using System.Text.Json;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseDynamicParameterNamingPolicy(string dataEle) : JsonNamingPolicy
{
    private string DataElementName { get; set; } = dataEle;

    public override string ConvertName(string name)
    {
        return name == "Data" ? DataElementName : name;
    }
}