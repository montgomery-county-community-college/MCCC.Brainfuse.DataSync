using System.Text.Json.Serialization;

namespace MCCC.Brainfuse.API.Wrapper.Models;

public class BrainfuseEventInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("attributes")]
    public BrainfuseAttribute[]? Attributes { get; set; }

    [JsonPropertyName("className")]
    public string? ClassName { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("control")]
    public BrainfuseControl? BrainfuseControl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("end")]
    public DateTimeOffset End { get; set; }

    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("eventDetailsID")]
    public string? EventDetailsId { get; set; }

    [JsonPropertyName("sessionMode")]
    public long SessionMode { get; set; }

    [JsonPropertyName("moderator")]
    public BrainfuseModerator? BrainfuseModerator { get; set; }

    [JsonPropertyName("offlineAttendanceAllowed")]
    public bool OfflineAttendanceAllowed { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("categoryStr")]
    public string? CategoryStr { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("scheduleID")]
    public long ScheduleId { get; set; }

    [JsonPropertyName("passedEndTime")]
    public bool PassedEndTime { get; set; }

    [JsonPropertyName("categoryID")]
    public long CategoryId { get; set; }

    [JsonPropertyName("attendeeList")]
    public BrainfuseModerator[]? AttendeeList { get; set; }
}