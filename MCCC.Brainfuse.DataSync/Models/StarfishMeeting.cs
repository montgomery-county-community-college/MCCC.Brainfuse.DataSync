using CsvHelper.Configuration.Attributes;

namespace MCCC.Brainfuse.DataSync.Models;

public class StarfishMeeting()
{
    [Name("integration_id")]
    public string? IntegrationId { get; set; }
    [Name("source")]
    public string? Source { get; set; }
    [Name("student_id")]
    public string? StudentId { get; set; }
    [Name("start_dt")]
    public string? StartDt { get; set; }
    [Name("end_dt")]
    public string? EndDt { get; set; }
    [Name("Location")]
    public string? Location { get; set; }
    [Name("Course")]
    public string? Course { get; set; }
    [Name("Reason")]
    public string? Reason { get; set; }
    [Name("provider_name")]
    public string? ProviderName { get; set; }
    [Name("Notes")]
    public string? Notes { get; set; }
}