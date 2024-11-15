using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseEvent : BrainfuseApiCallElement
{
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("eventID")]
    public long EventId { get; set; }

    [JsonPropertyName("requestID")]
    public long? RequestId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("userID")]
    public long UserId { get; set; }

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("accountID")]
    public long AccountId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("account")]
    public string? Account { get; set; }

    [JsonPropertyName("sisID")]
    public string SisId { get; set; }

    [JsonPropertyName("startDate")]
    public DateTimeOffset StartDate { get; set; }

    [JsonPropertyName("categoryID")]
    public long CategoryId { get; set; }

    [JsonPropertyName("subjectName")]
    public string? SubjectName { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public string? Location { get; set; }

    public string? LocationValue { get; set; }

    public string? Reason { get; set; }

    public string? Provider { get; set; }

    public bool Attended { get; set; }
}