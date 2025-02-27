using System.ComponentModel;
using MCCC.Colleague.SQL.Wrapper.Models;

namespace MCCC.Brainfuse.DataSync.Models;

public class ColleagueTutoringSessionData()
{
    public ColleagueTutoringSessionData(string studentId, string integrationId, string source) : this()
    {
        this.StudentId = studentId;
        this.IntegrationId = integrationId;
        this.Source = source;
    }
    public string IntegrationId { get; set; }
    public string Source { get; set; }
    public string? StudentId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string? Location { get; set; }
    public string? Subject { get; set; }
    public IEnumerable<string?>? CoursesForSubject { get; set; }
    public IEnumerable<ColleagueStudentEnrollment>? CoursesForStudent { get; set; }
    public string? Course { get; set; }
    public string? Reason { get; set; }
    public string? Provider { get; set; }
    public string? Notes { get; set; }
    public bool Attended { get; set; }
    public SessionTypes? SessionType { get; set; }

    public enum SessionTypes
    {
        [Description("Boost Attendance")]
        BoostAttendance,
        [Description("Live Session")]
        LiveSession,
        [Description("Writing Lab")]
        WritingLab
    }
}