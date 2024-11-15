using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseWritingLab : BrainfuseApiCallElement
{
    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("uid")]
    public long Uid { get; set; }

    [JsonPropertyName("courseName")]
    public string? CourseName { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("minutes")]
    public long Minutes { get; set; }

    [JsonPropertyName("collegeID")]
    public string? CollegeId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}