using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseLiveSessionElement : BrainfuseApiCallElement
{

    [JsonPropertyName("eventID")]
    public string? EventId { get; set; }

    [JsonPropertyName("requestID")]
    public long? RequestId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }

    [JsonPropertyName("courseName")]
    public string? CourseName { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("minutes")]
    public double Minutes { get; set; }

    [JsonPropertyName("collegeID")]
    public string? CollegeId { get; set; }

    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("courseId")]
    public string? CourseId { get; set; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("billedMins")]
    public double? BilledMins { get; set; }

    [JsonPropertyName("tutorFlag")]
    public string? TutorFlag { get; set; }
}