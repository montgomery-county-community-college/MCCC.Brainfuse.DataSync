namespace MCCC.Colleague.SQL.Wrapper.Models;

public class ColleagueStudentEnrollment(
    string colleagueId,
    string courseName,
    string courseSubject,
    string courseNumber,
    string courseSection)
{
    public string ColleagueId { get; set; } = colleagueId;
    public string CourseName { get; set; } = courseName;
    public string CourseSubject { get; set; } = courseSubject;
    public string CourseNumber { get; set; } = courseNumber;
    public string CourseSection { get; set; } = courseSection;
}